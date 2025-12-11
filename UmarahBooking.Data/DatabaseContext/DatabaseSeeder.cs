using UmarahBooking.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace UmarahBooking.Data.Seed
{
    /// <summary>
    /// Database seeder for initializing roles and default admin user
    /// Application Name: Manisik (Umrah & Hajj Booking System)
    /// </summary>
    public static class DatabaseSeeder
    {
        #region Application Info

        public const string ApplicationName = "UmarahBooking";
        public const string ApplicationDescription = "Umrah & Hajj Booking Management System";

        #endregion

        #region Role Definitions

        public static class Roles
        {
            public const string Admin = "Admin";
            public const string User = "User";
            public const string HotelManager = "HotelManager";
        }

        #endregion

        #region Default Admin Configuration

        private static class DefaultAdmin
        {
            public const string Email = "admin@manisik.com";
            public const string Password = "Admin@123456";
            public const string FullName = "System Administrator";
            public const string PhoneNumber = "+966500000000";
            public const string Country = "Saudi Arabia";
        }

        #endregion

        #region Seed Methods

        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            // Resolve required services
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DatabaseSeeder");

            try
            {
                logger.LogInformation("========================================");
                logger.LogInformation($"Starting {ApplicationName} Database Seeding");
                logger.LogInformation("========================================");

                // Seed Roles
                await SeedRoles(roleManager, logger);

                // Seed Default Admin
                await SeedAdminUser(userManager, logger);

                // Seed Hotels (User Data)
                var unitOfWork = serviceProvider.GetRequiredService<UmarahBooking.Core.Interfaces.IUnitOfWork>();
                await SeedHotels(unitOfWork, logger);

                logger.LogInformation("========================================");
                logger.LogInformation("Database Seeding Completed Successfully");
                logger.LogInformation("========================================");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        private static async Task SeedRoles(RoleManager<IdentityRole<int>> roleManager, ILogger logger)
        {
            logger.LogInformation("Seeding Roles...");

            var rolesToSeed = new[]
            {
                new { Name = Roles.Admin, Description = "System Administrator with full access" },
                new { Name = Roles.User, Description = "Standard user who can make bookings" },
                new { Name = Roles.HotelManager, Description = "Hotel manager with limited admin access" }
            };

            foreach (var role in rolesToSeed)
            {
                if (!await roleManager.RoleExistsAsync(role.Name))
                {
                    var identityRole = new IdentityRole<int>(role.Name);
                    var result = await roleManager.CreateAsync(identityRole);

                    if (result.Succeeded)
                        logger.LogInformation("? Role '{RoleName}' created successfully - {Description}", role.Name, role.Description);
                    else
                        logger.LogError("? Failed to create role '{RoleName}': {Errors}", role.Name, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
                else
                {
                    logger.LogInformation("? Role '{RoleName}' already exists - skipping", role.Name);
                }
            }
        }

        private static async Task SeedAdminUser(UserManager<ApplicationUser> userManager, ILogger logger)
        {
            logger.LogInformation("Seeding Default Admin User...");

            var existingAdmin = await userManager.FindByEmailAsync(DefaultAdmin.Email);
            if (existingAdmin != null)
            {
                logger.LogWarning("? Admin user '{Email}' already exists - skipping", DefaultAdmin.Email);
                return;
            }

            var adminUser = new ApplicationUser
            {
                UserName = DefaultAdmin.Email,
                Email = DefaultAdmin.Email,
                EmailConfirmed = true,
                FullName = DefaultAdmin.FullName,
                PhoneNumber = DefaultAdmin.PhoneNumber,
                PhoneNumberConfirmed = true,
                Country = DefaultAdmin.Country,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(adminUser, DefaultAdmin.Password);
            if (!createResult.Succeeded)
            {
                logger.LogError("? Failed to create admin user: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
                return;
            }

            var roleResult = await userManager.AddToRoleAsync(adminUser, Roles.Admin);
            if (roleResult.Succeeded)
            {
                logger.LogInformation("========================================");
                logger.LogInformation("? Default Admin User Created Successfully");
                logger.LogInformation("Email: {Email}", DefaultAdmin.Email);
                logger.LogInformation("Password: {Password}", DefaultAdmin.Password);
                logger.LogInformation("Role: {Role}", Roles.Admin);
                logger.LogWarning("? SECURITY WARNING: Please change the default admin password after first login!");
                logger.LogInformation("========================================");
            }
            else
            {
                logger.LogError("? Failed to assign Admin role to user: {Errors}", string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }
        }

        private static async Task SeedHotels(UmarahBooking.Core.Interfaces.IUnitOfWork unitOfWork, ILogger logger)
        {
            var existingHotels = await unitOfWork.Hotels.GetAllAsync();
            if (existingHotels.Any())
            {
                logger.LogInformation("Skipping Hotel Seeding: Data already exists.");
                return;
            }

            logger.LogInformation("Seeding Initial Hotels...");

            var hotels = new List<Hotel>
            {
                new Hotel
                {
                    Name = "Hilton Makkah",
                    HotelCity = UmarahBooking.Core.Enums.HotelCity.Makkah,
                    Address = "Al Haram Rd",
                    StarRating = 5,
                    DistanceToHaram = 200.00m,
                    Description = "Luxurious hotel near Haram",
                    ImageUrl = "/images/hotels/hilton-makkah.jpg",
                    IsActive = true
                },
                new Hotel
                {
                    Name = "Anwar Al Madinah",
                    HotelCity = UmarahBooking.Core.Enums.HotelCity.Madinah,
                    Address = "Prince Mohammed St",
                    StarRating = 4,
                    DistanceToHaram = 300.00m,
                    Description = "Near Prophet Mosque",
                    ImageUrl = "/images/hotels/anwar-madinah.jpg",
                    IsActive = true
                },
                new Hotel
                {
                    Name = "Swissotel Al Maqam",
                    HotelCity = UmarahBooking.Core.Enums.HotelCity.Makkah,
                    Address = "Abraj Al Bait",
                    StarRating = 4,
                    DistanceToHaram = 350.00m,
                    Description = "Connected to Abraj Complex",
                    ImageUrl = "/images/hotels/swissotel.jpg",
                    IsActive = true
                },
                new Hotel
                {
                    Name = "Dar Al Iman",
                    HotelCity = UmarahBooking.Core.Enums.HotelCity.Madinah,
                    Address = "King Fahd Rd",
                    StarRating = 5,
                    DistanceToHaram = 250.00m,
                    Description = "Luxury suites",
                    ImageUrl = "/images/hotels/dar-al-iman.jpg",
                    IsActive = true
                },
                new Hotel
                {
                    Name = "Conrad Makkah",
                    HotelCity = UmarahBooking.Core.Enums.HotelCity.Makkah,
                    Address = "King Abdulaziz Rd",
                    StarRating = 5,
                    DistanceToHaram = 150.00m,
                    Description = "Premium suites",
                    ImageUrl = "/images/hotels/conrad.jpg",
                    IsActive = true
                },
                new Hotel
                {
                    Name = "Pullman Zamzam",
                    HotelCity = UmarahBooking.Core.Enums.HotelCity.Makkah,
                    Address = "Abraj Al Bait",
                    StarRating = 4,
                    DistanceToHaram = 300.00m,
                    Description = "Modern design",
                    ImageUrl = "/images/hotels/pullman.jpg",
                    IsActive = true
                }
            };

            foreach (var hotel in hotels)
            {
                await unitOfWork.Hotels.AddAsync(hotel);
            }
            await unitOfWork.SaveChanges();

            logger.LogInformation($"? Seeded {hotels.Count} Hotels successfully.");
        }

        #endregion

        #region Extension Method for IHost

        public static async Task SeedDatabase(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                await SeedDatabase(services);
            }
            catch (Exception ex)
            {
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("DatabaseSeeder");
                logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        #endregion
    }
}

