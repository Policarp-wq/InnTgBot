using InnTgBot.ApiContracts;
using InnTgBot.Models;
using System.Text;
using System.Text.Json;

namespace InnTgBot.Services
{
    internal class INNService : IINNService
    {
        private const string API_URL = "https://suggestions.dadata.ru/suggestions/api/4_1/rs/findById/party";
        private readonly HttpClient _httpClient;
        private readonly string _innApiKey;
        public INNService(HttpClient httpClient, string innApiKey)
        {
            _httpClient = httpClient;
            _innApiKey = innApiKey;
        }

        public async Task<CompanyInfo?> GetCompanyInfo(string inn)
        {
            if (!IINNService.IsValid(inn))
                throw new FormatException($"Invalid form of the inn: {inn}");
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
                return null;
            try
            {
                var companyInfo = await response.Content.ReadAsStringAsync();
                using JsonDocument document = JsonDocument.Parse(companyInfo);
                var suggestions = document.RootElement.GetProperty("suggestions");
                if (suggestions.GetArrayLength() == 0)
                    return null;
                var root = suggestions[0];
                var name = root.GetProperty("value");
                var address = root.GetProperty("data").GetProperty("address").GetProperty("value");
                return new CompanyInfo(inn, name.ToString(), address.ToString());
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<CompanyInfo>> GetCompanyInfos(string[] inns)
        {
            //Отдельным листом не найденные
            List<CompanyInfo> res = [];
            foreach (var inn in inns)
            {
                try
                {
                    var info = await GetCompanyInfo(inn);
                    if (info != null)
                    {
                        res.Add(info);
                    }
                    else
                    {
                        res.Add(new CompanyInfo(inn, "Not found", "Not found"));
                    }
                }
                catch (FormatException)
                {
                    res.Add(new CompanyInfo(inn, "Wrong format for INN", "Wrong format for INN"));
                }
            }
            return res.OrderBy(c => c.Name).ToList();
        }
    }
}
