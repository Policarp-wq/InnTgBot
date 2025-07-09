using InnTgBot.Bot;
using InnTgBot.Services;

var tgApiKey = Environment.GetEnvironmentVariable("TG_BOT_API_KEY");
var innApiKey = Environment.GetEnvironmentVariable("INN_API_KEY");
if (tgApiKey == null)
    throw new Exception("Telegram bot api key is not provided");
if (innApiKey == null)
    throw new Exception("Inn service api key is not provided");

using HttpClient client = new();
var inn = new INNService(client, innApiKey);
TgBot bot = new(tgApiKey, inn);
if (await bot.Check())
    Console.WriteLine("Bot started working. \nPress Esc key to stop...");

while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
bot.Stop();
Console.WriteLine("Stopped.");
