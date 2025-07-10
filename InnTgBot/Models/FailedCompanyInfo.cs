using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InnTgBot.Models
{
    public record CompanyInfoResponse(string Inn, CompanyInfo? Info = null, string? ErrorMessage = null);
}
