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
var messageHandler = new BotMessageHandler(inn);
TgBot bot = new(tgApiKey, messageHandler.GetAnswerMessage);
bot.OnBotStop = async () =>
{
    foreach (var id in messageHandler.ClearInnRequests())
    {
        await bot.SendCustomMessage(new MessageInfo(id, "Bot is shutting down. Request for INN declined"));
    }
};
var cts = new CancellationTokenSource();
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true; 
    cts.Cancel();
    Console.WriteLine("Stopping...");
};

try
{
    if (await bot.Check())
        Console.WriteLine("Bot started working. \nPress Ctrl+C to stop...");
    await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (OperationCanceledException)
{

}

await bot.Stop();
Console.WriteLine("Stopped.");
