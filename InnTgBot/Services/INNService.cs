using InnTgBot.ApiContracts;
using InnTgBot.Models;
using System.Text;
using System.Text.Json;

namespace InnTgBot.Services
{
    public class INNService : IINNService
    {
        private const string API_URL = "https://suggestions.dadata.ru/suggestions/api/4_1/rs/findById/party";
        private readonly HttpClient _httpClient;
        private readonly string _innApiKey;
        public INNService(HttpClient httpClient, string innApiKey)
        {
            _httpClient = httpClient;
            _innApiKey = innApiKey;
        }

        public async Task<CompanyInfoResponse> GetCompanyInfo(string inn)
        {
            if (!IINNService.IsValid(inn))
                return new CompanyInfoResponse(inn, ErrorMessage: "Wrong format");
            HttpContent content = new StringContent(
                JsonSerializer.Serialize(new INNQuery(inn)),
                Encoding.UTF8,
                "application/json"
            );
            HttpRequestMessage httpRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri(API_URL),
                Method = HttpMethod.Post,
                Content = content
            };
            httpRequest.Headers.Add("Accept", "application/json");
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", _innApiKey);
            var response = await _httpClient.SendAsync(httpRequest);
            if (!response.IsSuccessStatusCode)
                return new CompanyInfoResponse(inn, ErrorMessage: "Failed to get response from the inn api service");
            try
            {
                var companyInfo = await response.Content.ReadAsStringAsync();
                using JsonDocument document = JsonDocument.Parse(companyInfo);
                var suggestions = document.RootElement.GetProperty("suggestions");
                if (suggestions.GetArrayLength() == 0)
                    return new CompanyInfoResponse(inn, ErrorMessage: "Not found");
                var root = suggestions[0];
                var name = root.GetProperty("value");
                var address = root.GetProperty("data").GetProperty("address").GetProperty("value");
                return new CompanyInfoResponse(inn, new CompanyInfo(inn, name.ToString(), address.ToString()));
            }
            catch (Exception)
            {
                return new CompanyInfoResponse(inn, ErrorMessage: "Not found");
            }
        }

        public async Task<List<CompanyInfoResponse>> GetCompanyInfos(string[] inns)
        {
            var tasks = inns.Select(inn => GetCompanyInfo(inn));
            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }
    }
}
