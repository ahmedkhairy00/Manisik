using Manasik.Infrastructure.Data; // ✅ مهم علشان كلاس Auth
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
using System.Text;

namespace Manisik
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ===========================================
            // 1️⃣ Database Connection (EF Core)
            // ===========================================
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ===========================================
            // 2️⃣ Identity Configuration
            // ===========================================
            builder.Services.AddIdentity<Auth, IdentityRole<int>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // ===========================================
            // 3️⃣ JWT Authentication Configuration
            // ===========================================
            var jwtConfig = builder.Configuration.GetSection("Jwt");
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

            // 🧩 Auth
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
    }
}
