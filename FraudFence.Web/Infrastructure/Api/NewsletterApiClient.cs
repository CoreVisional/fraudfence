using FraudFence.EntityModels.Dto.Newsletter;
using static System.Net.WebRequestMethods;

namespace FraudFence.Web.Infrastructure.Api
{
    public class NewsletterApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public NewsletterApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["PublisherApiBase"]!;
        }

        public async Task<List<NewsletterDTO>> GetAllAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<NewsletterDTO>>($"{_baseUrl}/newsletters");

            return response!;
        }

        public async Task<dynamic?> GetAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<dynamic>($"{_baseUrl}/newsletters/{id}");
        }

        public async Task<NewsletterArticlesDTO?> GetWithArticlesAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<NewsletterArticlesDTO>($"{_baseUrl}/newsletters/{id}");
        }

        public async Task CreateAsync(CreateNewsletterDTO dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/newsletters", dto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API call failed: {response.StatusCode} - {error}");
            }
        }

        public async Task UpdateAsync(int id, UpdateNewsletterDTO dto)
        {
            var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/newsletters/{id}", dto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API call failed: {response.StatusCode} - {error}");
            }
        }

        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/newsletters/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API call failed: {response.StatusCode} - {error}");
            }
        }
    }
}
