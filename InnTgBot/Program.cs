using InnTgBot.Bot;

var apiKey = Environment.GetEnvironmentVariable("TG_BOT_API_KEY");
if (apiKey == null)
    throw new Exception("Api key is not provided");
TgBot bot = new(apiKey);
if(await bot.Check())
    Console.WriteLine("Bot started working. \nPress any key to stop...");
Console.ReadLine();
bot.Stop();
