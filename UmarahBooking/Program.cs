using UmarahBooking.Core.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using System.Text;
using System.Threading.RateLimiting;
using UmarahBooking.Core.Interfaces;
using UmarahBooking.Core.Mapping;
using UmarahBooking.Core.Services;
using UmarahBooking.Data.Repositories;
using UmarahBooking.Data.Seed;
using UmarahBooking.Data.DatabaseContext;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace UmarahBooking
{   
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Controllers with Newtonsoft JSON
            builder.Services.AddControllers().AddNewtonsoftJson(options =>
            {
                // Use camel case to match the frontend TypeScript interfaces
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            });
            builder.Services.AddEndpointsApiExplorer();

            // Configure AutoMapper
            builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

            // Configure Swagger with JWT Authentication
            ConfigureSwagger(builder.Services);

            // Configure Database
            if (builder.Environment.IsEnvironment("Testing"))
            {
                // Use InMemory database for testing
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            }
            else
            {
                // Use SQL Server for other environments
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(
                        builder.Configuration.GetConnectionString("DefaultConnection"),
                        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
            }

            // -----------------------------------------------------
            // *** ChatBot HttpClient (Copied & Improved from your first code) ***
            // -----------------------------------------------------
            builder.Services.AddHttpClient("ChatBot", (sp, client) =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var baseUrl = config["ChatBot:ApiUrl"];

                if (string.IsNullOrWhiteSpace(baseUrl))
                    throw new InvalidOperationException("ChatBot:ApiUrl is not configured.");

                client.BaseAddress = new Uri(baseUrl);

                var apiKey = config["ChatBot:ApiKey"];
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                }

                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            // -----------------------------------------------------

            // Register HttpClient Factory
            builder.Services.AddHttpClient();

            // Register Application Services
            RegisterServices(builder.Services);

            // Register background expiration hosted service
            builder.Services.AddHostedService<BookingExpirationService>();

            // Configure Stripe
            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            // Configure Identity
            ConfigureIdentity(builder.Services);

            // Configure Authentication & Authorization
            ConfigureAuthentication(builder.Services, builder.Configuration);
            ConfigureAuthorization(builder.Services);

            // Configure Rate Limiting
            ConfigureRateLimiting(builder.Services);

            // Configure Memory Cache for server-side caching
            builder.Services.AddMemoryCache();

            // Configure CORS
            builder.Services.AddCors(opt =>
            {
                opt.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            var app = builder.Build();

            // Seed Database
            await SeedDatabase(app);

            // Configure HTTP Request Pipeline
            ConfigurePipeline(app);

            app.Run();
        }

        private static void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "UmarahBooking API",
                    Version = "v1",
                    Description = "API for Umarah Booking System"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid token.\n\nExample: \"Bearer eyJhbGci...\""
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
        }


        private static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IHotelService, HotelService>();
            services.AddScoped<IBookingHotelService, BookingHotelService>();
            services.AddScoped<IBookingGroundTransportService, BookingGroundTransportService>();
            services.AddScoped<IBookingInternationalTransportService, BookingInternationalTransportService>();
            services.AddScoped<IInternationalTransportBookingService, InternationationalTransportBookingService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ChatBotService>();
            services.AddSingleton<ChatMemoryService>();
        }

        private static void ConfigureIdentity(IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                // User settings
                options.User.RequireUniqueEmail = true;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        }

        private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
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
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
                    ClockSkew = TimeSpan.Zero
                };

                // Support both Authorization header and Cookie authentication
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // First, try to get token from Authorization header (for Swagger, API clients)
                        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = authHeader.Substring("Bearer ".Length).Trim();
                        }
                        // If not in header, try to get from cookie (for browser-based clients)
                        else if (context.Request.Cookies.ContainsKey("authToken"))
                        {
                            context.Token = context.Request.Cookies["authToken"];
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        }

        private static void ConfigureAuthorization(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
                options.AddPolicy("HotelManagerOnly", policy => policy.RequireRole("HotelManager"));
            });
        }

        private static void ConfigureRateLimiting(IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                // Auth endpoints rate limit (stricter)
                options.AddPolicy("auth", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));

                // Payment endpoints rate limit
                options.AddPolicy("payment", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));

                // Global rate limiter (more permissive)
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
                    return RateLimitPartition.GetTokenBucketLimiter(ip, _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 100,
                        TokensPerPeriod = 20,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
                });

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });
        }

        private static async Task SeedDatabase(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            await DatabaseSeeder.SeedDatabase(services);
        }

        private static void ConfigurePipeline(WebApplication app)
        {
            // Swagger (only in Development)
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Manisik API v1");
                    c.RoutePrefix = "swagger";
                });
            }
            else
            {
                app.UseHsts();
            }

            // CORS (Must be before Rate Limiting and Auth to handle preflight/errors correctly)
            // Enable static files to serve images
app.UseStaticFiles();

app.UseCors("AllowAll");

            // Rate limiting
            app.UseRateLimiter();

            // HTTPS Redirection
            app.UseHttpsRedirection();

            // Security headers
            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-Frame-Options"] = "DENY";
                context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
                // CSP allows: Stripe, Google Fonts, common image CDNs, and inline styles for Angular
                // Added worker-src for Stripe
                context.Response.Headers["Content-Security-Policy"] = 
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://js.stripe.com https://m.stripe.network; " +
                    "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
                    "font-src 'self' https://fonts.gstatic.com; " +
                    "img-src 'self' data: blob: https://images.unsplash.com https://*.stripe.com; " +
                    "worker-src 'self' blob:; " + 
                    "frame-src 'self' https://js.stripe.com https://hooks.stripe.com; " +
                    "connect-src 'self' http://localhost:* https://api.stripe.com https://m.stripe.network;";
                await next();
            });

            // Static files
            // app.UseStaticFiles(); // Already configured above

            // Authentication & Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Controllers
            app.MapControllers();
        }
    }
}
