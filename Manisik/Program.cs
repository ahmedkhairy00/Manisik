using Manasik.Infrastructure.Data;// ✅ مهم علشان كلاس Auth
using Manisik.Interfaces;
using Manisik.Mapping;
using Manisik.Models;
using Manisik.Repositories;
using Manisik.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;


namespace Manisik
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =========================
            // Configuration providers
            // =========================
            // Use user-secrets in Development for local secrets (dotnet user-secrets)
            if (builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddUserSecrets<Program>(optional: true);
            }

            // If KeyVault name is provided, load secrets from Azure Key Vault (production)
            var keyVaultName = builder.Configuration["KeyVault:Name"];
            if (!string.IsNullOrEmpty(keyVaultName))
            {
                var kvUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
                builder.Configuration.AddAzureKeyVault(kvUri, new DefaultAzureCredential());
            }

            // ===========================================
            // 1️⃣ Database Connection (EF Core)
            // ===========================================
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ===========================================
            // 2️⃣ Identity Configuration
            // ===========================================
            // Keep single Identity registration using Auth and int-based roles
            builder.Services.AddIdentity<Auth, IdentityRole<int>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // ===========================================
            // 3️⃣ JWT Authentication Configuration
            // ===========================================
            var jwtConfig = builder.Configuration.GetSection("Jwt");

            // Register JwtService with DI (scoped because it depends on UserManager which is scoped)
            builder.Services.AddScoped<JwtService>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfig["Issuer"],
                    ValidAudience = jwtConfig["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtConfig["Key"]))
                };
            });

            // ===========================================
            // 4️⃣ AutoMapper Registration
            // ===========================================
            builder.Services.AddAutoMapper(typeof(MappingProfile));

            // ===========================================
            // 5️⃣ Controllers & Swagger
            // ===========================================
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Manasik API", Version = "v1" });
               

                // دعم JWT في Swagger UI
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "أدخل توكن JWT هنا (بدون كلمة Bearer)",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });


            // ===========================================
            // 6️⃣ Dependency Injection (Repositories & Services)
            // ===========================================
            builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<IHotelRepository, HotelRepository>();
            builder.Services.AddScoped<ITransportRepository, TransportRepository>();
            builder.Services.AddScoped<IUmrahBookingRepository, UmrahBookingRepository>();
            builder.Services.AddScoped<HotelService>();
            builder.Services.AddScoped<TransportService>();
            builder.Services.AddScoped<UmrahBookingService>();




            // ===========================================
            // 7️⃣ CORS Policy
            // ===========================================
            builder.Services.AddCors(opt =>
            {
                opt.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // ===========================================
            // 8️⃣ Serilog Logging
            // ===========================================
            builder.Host.UseSerilog((context, config) =>
                config.ReadFrom.Configuration(context.Configuration));

            // ===========================================
            // 9️⃣ Build App
            // ===========================================
            var app = builder.Build();

            // ===========================================
            // Seed default roles + admin user (synchronous call from Main)
            // ===========================================
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    CreateDefaultRolesAndAdminAsync(services, app.Configuration).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    app.Logger.LogError(ex, "An error occurred while seeding default roles/admin.");
                }
            }

            // ===========================================
            // 🔟 Middleware Pipeline
            // ===========================================
            app.UseCors("AllowAll");
            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            // Swagger UI
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Manasik API v1");
                c.RoutePrefix = string.Empty;
            });

            app.MapControllers();

            // ===========================================
            // 🔥 Run App
            // ===========================================
            app.Run();
        }

        // ===========================================
        // Seed helper: creates roles and a default admin
        // ===========================================
        private static async Task CreateDefaultRolesAndAdminAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            // Use the int-based IdentityRole to match your Identity setup
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Auth>>();

            string[] roleNames = { "Admin", "User", "HotelManager", "TransportProvider", "GroundTransportProvider" };

            foreach (var roleName in roleNames)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    var role = new IdentityRole<int> { Name = roleName };
                    await roleManager.CreateAsync(role);
                }
            }

            // Admin credentials — prefer to set these in configuration (appsettings or secrets)
            var adminEmail = configuration["AdminUser:Email"] ?? "admin@manasik.local";
            var adminPassword = configuration["AdminUser:Password"] ?? "Admin@12345"; // change before production

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var admin = new Auth
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(admin, adminPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
                // optionally handle createResult.Errors
            }
            else
            {
                // ensure user is in Admin role
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
