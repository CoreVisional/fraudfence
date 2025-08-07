using FraudFence.EntityModels.Dto;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;


namespace FraudFence.Service
{
    public sealed class UserService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;
        private readonly Data.ApplicationDbContext _context;

        public UserService(IHttpClientFactory httpClientFactory, Microsoft.Extensions.Configuration.IConfiguration configuration, Data.ApplicationDbContext context)
        {
            _httpClient = httpClientFactory.CreateClient("UsersApi");
            _apiBaseUrl = configuration["AWS:ApiGateway:UserManagementApiUrl"]!;
            _context = context;
        }

        public async Task<List<UserViewModel>> GetUsersAsync()
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/users");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<UserViewModel>>() ?? new List<UserViewModel>();
        }

        public async Task<UserViewModel?> GetUserByIdAsync(string id)
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/users/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserViewModel>();
            }
            return null;
        }

        public async Task<bool> CreateUserAsync(CreateUserViewModel model)
        {
            var registrationDto = new RegistrationDTO(model.Name, model.Email, model.Password);
            var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}/register", registrationDto);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();
            var userId = responseBody.GetProperty("userId").GetString();

            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            var groupData = new { groupName = model.Role };
            await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}/users/{userId}/groups", groupData);

            // Also update the user's phone number in the local DB
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.PhoneNumber = model.PhoneNumber;
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> EditUserAsync(EditUserViewModel model)
        {
            var response = await _httpClient.PutAsJsonAsync($"{_apiBaseUrl}/users/{model.Id}", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}/users/{id}");
            return response.IsSuccessStatusCode;
        }

        // The concept of "locking" a user is handled by disabling them in Cognito.
        // This will be a future implementation if required.
        public Task ToggleActiveAsync(string id)
        {
            // To be implemented: This would involve a call to an "update" or "disable" endpoint
            // in our Users API, which would then call AdminDisableUser in Cognito.
            return Task.CompletedTask;
        }
    }
}
