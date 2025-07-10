using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace InnTgBot.Bot
{
    public class TgBot
    {
        private readonly string _tgApiKey;
        private readonly TelegramBotClient _client;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public Func<Message, Task<MessageInfo>> GetAnswerMessage;
        public Func<Task>? OnBotStop;
        public TgBot(string apiKey, Func<Message, Task<MessageInfo>> getAnswerMessage)
        {
            _tgApiKey = apiKey;
            _cancellationTokenSource = new CancellationTokenSource();
            _client = new TelegramBotClient(_tgApiKey, cancellationToken: _cancellationTokenSource.Token);
            _client.OnMessage += OnMessageRecieved;
            _client.OnError += OnError;
            GetAnswerMessage = getAnswerMessage;
        }

        private async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception.Message);
        }

        public async Task<bool> Check()
        {
            return await _client.TestApi();
        }
        public async Task Stop()
        {
            if (OnBotStop != null)
                await OnBotStop.Invoke();
            _cancellationTokenSource.Cancel();
        }

        private async Task OnMessageRecieved(Message message, UpdateType type)
        {
            Console.WriteLine($"[{message.Date}]: Received from {(message.From == null ? "Chat" : message.From.Username)}: {message.Text}");
            await SendCustomMessage(await GetAnswerMessage.Invoke(message));

        }
        public async Task<Message> SendCustomMessage(MessageInfo info)
        {
            var res = await _client.SendMessage(info.ChatId, info.Text, info.ParseMode, replyMarkup: info.ReplyMarkup);
            Console.WriteLine($"[{res.Date}]: Sent to {res.Chat.Username}: {res.Text}");
            return res;
        }

    }
}
