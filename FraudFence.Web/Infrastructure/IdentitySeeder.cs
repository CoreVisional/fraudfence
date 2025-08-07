using FraudFence.Data;
using FraudFence.EntityModels.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace FraudFence.Web.Infrastructure
{
    public class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var sysEmail = configuration["SystemUser:Email"]!;
            var sysPassword = configuration["SystemUser:Password"]!;

            var sysUser = await userManager.FindByEmailAsync(sysEmail);

            if (sysUser == null)
            {
                sysUser = new ApplicationUser
                {
                    Name = "SYSTEM",
                    UserName = "system",
                    Email = sysEmail,
                    EmailConfirmed = true
                };

                var create = await userManager.CreateAsync(sysUser, sysPassword);

                if (!create.Succeeded) throw new Exception("Failed to create system user: " + string.Join("; ", create.Errors.Select(e => e.Description)));
            }

            if (!await userManager.IsInRoleAsync(sysUser, "Admin")) await userManager.AddToRoleAsync(sysUser, "Admin");

            await userManager.AddClaimsAsync(sysUser,
            [
                new Claim("FullName", sysUser.Name),
                new Claim(ClaimTypes.Email, sysUser.Email!)
            ]);

        }
    }
}
