using FraudFence.EntityModels.Dto.ScamReport;

namespace FraudFence.Web.Infrastructure.Api
{

    public sealed class ScamReportApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ScamReportApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ScamReportApiBase"]!;
        }

        public async Task<ScamReportDTO?> GetAsync(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<ScamReportDTO>($"{_baseUrl}/scamReports/{id}");
            return response!;
        }

        public async Task<List<ScamReportDTO>> GetReportsByUserIdAsync(string userId)
        {
            var response = await _httpClient.GetFromJsonAsync<List<ScamReportDTO>>(
                $"{_baseUrl}/scamReports/owner/{userId}"
            );

            return response ?? new List<ScamReportDTO>();
        }

        public async Task<ScamReportDTO?> CreateAsync(CreateScamReportDTO scamReportDTO)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/scamReports", scamReportDTO);

            if (response.IsSuccessStatusCode)
            {
                var scamReportDto = await response.Content.ReadFromJsonAsync<ScamReportDTO>();
                return scamReportDto;
            }
            
            return null;
        }
    }

}