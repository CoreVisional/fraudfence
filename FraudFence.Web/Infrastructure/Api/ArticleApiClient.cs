using FraudFence.EntityModels.Dto.Article;

namespace FraudFence.Web.Infrastructure.Api
{
    public sealed class ArticleApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ArticleApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["PublisherApiBase"]!;
        }

        public async Task<List<ArticleDTO>> GetAllAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<ArticleDTO>>($"{_baseUrl}/articles");
            return response!;
        }

        public async Task<ArticleDTO?> GetAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<ArticleDTO>($"{_baseUrl}/articles/{id}");
        }

        public async Task CreateAsync(CreateArticleDTO dto)
        {
            await _httpClient.PostAsJsonAsync($"{_baseUrl}/articles", dto);
        }

        public async Task UpdateAsync(int id, CreateArticleDTO dto)
        {
            await _httpClient.PutAsJsonAsync($"{_baseUrl}/articles/{id}", dto);
        }

        public async Task DeleteAsync(int id)
        {
            await _httpClient.DeleteAsync($"{_baseUrl}/articles/{id}");
        }
    }
}
