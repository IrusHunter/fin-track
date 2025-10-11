using System.Security.Claims;
using IdentityModel;
using AuthServer.Data;
using AuthServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace AuthServer;

public class SeedData
{
    public static void EnsureSeedData(WebApplication app)
    {
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();

            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Створюємо ролі
            string[] roles = new[] { "user", "admin", "accountant" };
            foreach (var role in roles)
            {
                if (!roleMgr.RoleExistsAsync(role).Result)
                {
                    roleMgr.CreateAsync(new IdentityRole(role)).Wait();
                }
            }

            // 2. Створюємо користувачів
            var users = new List<(string Username, string FullName, string Email, string Phone, string Role)>
            {
                ("admin", "Admin User", "admin@example.com", "+380501234567", "admin"),
                ("accountant", "Accountant User", "accountant@example.com", "+380501234568", "accountant"),
                ("user", "Regular User", "user@example.com", "+380501234569", "user")
            };

            foreach (var u in users)
            {
                var appUser = userMgr.FindByNameAsync(u.Username).Result;
                if (appUser == null)
                {
                    appUser = new ApplicationUser
                    {
                        UserName = u.Username,
                        FullName = u.FullName,
                        Email = u.Email,
                        PhoneNumber = u.Phone,
                        EmailConfirmed = true,
                        PhoneNumberConfirmed = true
                    };

                    var result = userMgr.CreateAsync(appUser, "Password123!").Result;
                    if (!result.Succeeded) throw new Exception(result.Errors.First().Description);

                    // Додаємо роль
                    userMgr.AddToRoleAsync(appUser, u.Role).Wait();
                }
            }
        }
    }
}
