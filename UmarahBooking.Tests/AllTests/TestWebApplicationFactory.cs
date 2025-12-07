using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Manisik.Models;

namespace UmarahBooking.Tests.TestInfrastructure
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Ensure application registers test DB provider
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registrations
                var descriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) || d.ServiceType == typeof(ApplicationDbContext)).ToList();
                foreach (var d in descriptors)
                {
                    services.Remove(d);
                }

                // Use a unique in-memory database name per test factory instance
                var dbName = "TestDb_" + System.Guid.NewGuid().ToString("N");

                // Add ApplicationDbContext using in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });

                // Build the service provider and seed the database
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;

                    // Seed Identity roles and admin user for tests
                    var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole<int>>>();

                    var roles = new[] { "Admin", "User", "HotelManager" };

                    // Deduplicate roles by NormalizedName to avoid EF InMemory SingleOrDefault failures
                    try
                    {
                        var existingRoles = roleManager.Roles.ToList();
                        var dupGroups = existingRoles.GroupBy(r => r.NormalizedName).Where(g => g.Count() > 1).ToList();
                        foreach (var g in dupGroups)
                        {
                            var keep = g.First();
                            foreach (var dup in g.Skip(1))
                            {
                                try { roleManager.DeleteAsync(dup).GetAwaiter().GetResult(); } catch { }
                            }
                        }
                    }
                    catch { }

                    foreach (var r in roles)
                    {
                        try
                        {
                            var exists = roleManager.RoleExistsAsync(r).GetAwaiter().GetResult();
                            if (!exists)
                            {
                                roleManager.CreateAsync(new IdentityRole<int>(r)).GetAwaiter().GetResult();
                            }
                        }
                        catch { /* ignore transient errors in test host */ }
                    }

                    // Create default admin user if not exists
                    var adminEmail = "testadmin@example.com";

                    // Ensure uniqueness: if multiple users with same email exist, remove duplicates
                    var usersWithEmail = userManager.Users.Where(u => u.Email == adminEmail).ToList();
                    if (usersWithEmail.Count > 1)
                    {
                        // Keep first, remove rest
                        var toKeep = usersWithEmail.First();
                        foreach (var dupe in usersWithEmail.Skip(1))
                        {
                            try { userManager.DeleteAsync(dupe).GetAwaiter().GetResult(); } catch { }
                        }
                    }

                    var admin = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
                    if (admin == null)
                    {
                        admin = new ApplicationUser
                        {
                            UserName = adminEmail,
                            Email = adminEmail,
                            FullName = "Test Admin",
                            CreatedAt = DateTime.UtcNow
                        };

                        var createResult = userManager.CreateAsync(admin, "P@ssw0rd1!").GetAwaiter().GetResult();
                        if (createResult.Succeeded)
                        {
                            try
                            {
                                userManager.AddToRoleAsync(admin, "Admin").GetAwaiter().GetResult();
                            }
                            catch { }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to create admin user: {string.Join(',', createResult.Errors.Select(e=>e.Description))}");
                        }
                    }
                    else
                    {
                        // Ensure admin has Admin role (idempotent)
                        try
                        {
                            var rolesOfUser = userManager.GetRolesAsync(admin).GetAwaiter().GetResult();
                            if (!rolesOfUser.Contains("Admin", System.StringComparer.OrdinalIgnoreCase))
                            {
                                userManager.AddToRoleAsync(admin, "Admin").GetAwaiter().GetResult();
                            }
                        }
                        catch { }
                    }
                }
            });
        }
    }
}
