using InnTgBot.Models;

namespace InnTgBot.Services
{
    public interface IINNService
    {
        public static bool IsValid(string inn)
        {
            if (string.IsNullOrWhiteSpace(inn) || !inn.All(char.IsDigit))
                return false;

            if (inn.Length == 10)
            {
                int[] coeffs = { 2, 4, 10, 3, 5, 9, 4, 6, 8 };
                int sum = 0;
                for (int i = 0; i < 9; i++)
                    sum += coeffs[i] * (inn[i] - '0');
                int control = (sum % 11) % 10;
                return control == (inn[9] - '0');
            }
            else if (inn.Length == 12)
            {
                int[] coeffs1 = { 7, 2, 4, 10, 3, 5, 9, 4, 6, 8 };
                int[] coeffs2 = { 3, 7, 2, 4, 10, 3, 5, 9, 4, 6, 8 };

                int sum1 = 0;
                for (int i = 0; i < 10; i++)
                    sum1 += coeffs1[i] * (inn[i] - '0');
                int control1 = (sum1 % 11) % 10;

                int sum2 = 0;
                for (int i = 0; i < 11; i++)
                    sum2 += coeffs2[i] * (inn[i] - '0');
                int control2 = (sum2 % 11) % 10;

                return control1 == (inn[10] - '0') && control2 == (inn[11] - '0');
            }

            return false;
        }
        public Task<CompanyInfo?> GetCompanyInfo(string number);
        public Task<List<CompanyInfo>> GetCompanyInfos(string[] inn);
    }
}
