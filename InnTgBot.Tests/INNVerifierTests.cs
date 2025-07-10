using InnTgBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InnTgBot.Tests
{
    public class INNVerifierTests
    {
        [Theory]
        [InlineData("7709439560", true)]
        [InlineData("7707083893", true)]
        [InlineData("770708383", false)]
        [InlineData("", false)]
        [InlineData("    ", false)]
        [InlineData("фвфав", false)]
        [InlineData("afaf", false)]
        public void INNVerifiesCorrecty(string inn, bool isCorrect)
        {
            Assert.Equal(isCorrect, IINNService.IsValid(inn));
        }
    }
}
