using FraudFence.Data;
using FraudFence.EntityModels.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

            if (!await userManager.IsInRoleAsync(sysUser, "Publisher")) await userManager.AddToRoleAsync(sysUser, "Publisher");

            await userManager.AddClaimsAsync(sysUser,
            [
                new Claim("FullName", sysUser.Name),
                new Claim(ClaimTypes.Email, sysUser.Email!)
            ]);

            await SeedConsumerUsersAsync(userManager, context);
        }

        private static async Task SeedConsumerUsersAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            var scamCategories = await context.ScamCategories.ToListAsync();

            if (!scamCategories.Any())
            {
                return;
            }

            var consumer1Email = "concussion239@gmail.com";
            var consumer1 = await userManager.FindByEmailAsync(consumer1Email);

            if (consumer1 == null)
            {
                consumer1 = new ApplicationUser
                {
                    Name = "John",
                    UserName = "john",
                    Email = consumer1Email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(consumer1, "Consumer123!");
                if (!createResult.Succeeded)
                {
                    throw new Exception("Failed to create consumer1: " + string.Join("; ", createResult.Errors.Select(e => e.Description)));
                }

                if (!await userManager.IsInRoleAsync(consumer1, "Consumer")) await userManager.AddToRoleAsync(consumer1, "Consumer");

                await userManager.AddClaimsAsync(consumer1,
                [
                    new Claim("FullName", consumer1.Name),
                   new Claim(ClaimTypes.Email, consumer1.Email!)
                ]);

                await CreateSubscriptionSettings(context, consumer1.Id, scamCategories, subscribeToAll: true);
            }

            var consumer2Email = "consumer2@example.com";
            var consumer2 = await userManager.FindByEmailAsync(consumer2Email);

            if (consumer2 == null)
            {
                consumer2 = new ApplicationUser
                {
                    Name = "Alice",
                    UserName = "alice",
                    Email = consumer2Email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(consumer2, "Consumer456!");
                if (!createResult.Succeeded)
                {
                    throw new Exception("Failed to create consumer2: " + string.Join("; ", createResult.Errors.Select(e => e.Description)));
                }

                if (!await userManager.IsInRoleAsync(consumer2, "Consumer")) await userManager.AddToRoleAsync(consumer2, "Consumer");

                await userManager.AddClaimsAsync(consumer2,
                [
                    new Claim("FullName", consumer2.Name),
                   new Claim(ClaimTypes.Email, consumer2.Email!)
                ]);

                await CreateSubscriptionSettings(context, consumer2.Id, scamCategories, subscribeToAll: false);
            }
        }

        private static async Task CreateSubscriptionSettings(ApplicationDbContext context, int userId,
            List<ScamCategory> scamCategories, bool subscribeToAll)
        {
            if (subscribeToAll)
            {
                foreach (var category in scamCategories)
                {
                    var setting = new Setting
                    {
                        UserId = userId,
                        ScamCategoryId = category.Id,
                        Subscribed = true,
                        CreatedAt = DateTime.Now,
                        LastModified = DateTime.Now
                    };

                    context.Settings.Add(setting);
                }
            }
            else
            {
                if (scamCategories.Count != 0)
                {
                    var setting = new Setting
                    {
                        UserId = userId,
                        ScamCategoryId = scamCategories.First().Id,
                        Subscribed = true,
                        CreatedAt = DateTime.Now,
                        LastModified = DateTime.Now
                    };

                    context.Settings.Add(setting);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
