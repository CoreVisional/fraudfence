using FraudFence.Data;
using FraudFence.EntityModels.Dto;
using FraudFence.EntityModels.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace FraudFence.Web.Infrastructure
{
    public class CognitoSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("UsersApi");

            var apiBaseUrl = configuration["AWS:ApiGateway:UserManagementApiUrl"];

            // Seed Admin User
            var adminEmail = configuration["SystemUser:Email"]!;
            var adminPassword = configuration["SystemUser:Password"]!;
            await SeedUserAsync(httpClient, apiBaseUrl, context, "SYSTEM", adminEmail, adminPassword, "Admin", true);

            // Seed Consumer Users
            await SeedUserAsync(httpClient, apiBaseUrl, context, "John", "concussion239@gmail.com", "Consumer123!", "Consumer", true);
            await SeedUserAsync(httpClient, apiBaseUrl, context, "Alice", "consumer2@example.com", "Consumer456!", "Consumer", false);
        }

        private static async Task SeedUserAsync(HttpClient httpClient, string apiBaseUrl, ApplicationDbContext context, string name, string email, string password, string role, bool subscribeToAll)
        {
            // Check if user exists in local DB first to avoid unnecessary API calls
            if (await context.Users.AnyAsync(u => u.Email == email))
            {
                return;
            }

            var registrationDto = new RegistrationDTO(name, email, password);
            var response = await httpClient.PostAsJsonAsync($"{apiBaseUrl}/register", registrationDto);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();
                var userId = responseBody.GetProperty("userId").GetString();

                if (!string.IsNullOrEmpty(userId))
                {
                    var newUser = new ApplicationUser
                    {
                        Id = userId,
                        Name = name,
                        Email = email,
                        CreatedAt = DateTime.UtcNow,
                        LastModified = DateTime.UtcNow,
                        IsDisabled = false
                    };
                    context.Users.Add(newUser);
                    await context.SaveChangesAsync();

                    // Add user to group
                    var groupData = new { groupName = role };
                    await httpClient.PostAsJsonAsync($"{apiBaseUrl}/users/{userId}/groups", groupData);

                    // Seed subscription settings for consumers
                    if (role == "Consumer")
                    {
                        var scamCategories = await context.ScamCategories.ToListAsync();
                        await CreateSubscriptionSettings(context, userId, scamCategories, subscribeToAll);
                    }
                }
            }
            else
            {
                // Log the error or handle it as needed
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create user {email}. Status: {response.StatusCode}, Details: {error}");
            }
        }

        private static async Task CreateSubscriptionSettings(ApplicationDbContext context, string userId, List<ScamCategory> scamCategories, bool subscribeToAll)
        {
            if (subscribeToAll)
            {
                foreach (var category in scamCategories)
                {
                    context.Settings.Add(new Setting { UserId = userId, ScamCategoryId = category.Id, Subscribed = true });
                }
            }
            else if (scamCategories.Any())
            {
                context.Settings.Add(new Setting { UserId = userId, ScamCategoryId = scamCategories.First().Id, Subscribed = true });
            }
            await context.SaveChangesAsync();
        }
    }
}