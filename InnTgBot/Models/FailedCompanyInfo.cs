namespace InnTgBot.Models
{
    public record CompanyInfoResponse(string Inn, CompanyInfo? Info = null, string? ErrorMessage = null);
}
