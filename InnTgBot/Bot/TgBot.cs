using InnTgBot.Services;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace InnTgBot.Bot
{
    public class TgBot
    {
        public static string WELCOME_MESSAGE = "Hello! This bot can give you information about companies by ther INN(ru abbr)";
        public static string CREATOR_INFO = "My creator is Eugene Tatarkin. Useful links:";
        public static string HELP_INFO = "Available commands";
        public static string INN_INFO = "Send one or more INN numbers separated by spaces";

        public const string INIT_COMMAND = "/start";
        public const string CREATOR_INFO_COMMAND = "/hello";
        public const string HELP_COMMAND = "/help";
        public const string INN_COMMAND = "/inn";

        public static InlineKeyboardButton[] CREDENTIALS =
                [
                    InlineKeyboardButton.WithUrl("GitHub", "https://github.com/Policarp-wq"),
                    InlineKeyboardButton.WithUrl("hh.ru", "https://hh.ru/resume/17c21b2fff0e996d490039ed1f454f75446353"),
                    InlineKeyboardButton.WithCopyText("email", "tatarkin-evgeniy@mail.ru")
                ];
        private static HashSet<long> _innRequests = new();
        public static string[] COMMANDS_LIST =
                [
                    HELP_COMMAND,
                    CREATOR_INFO_COMMAND,
                    INN_COMMAND,
                    "/last",
                ];

        private readonly string _tgApiKey;
        private readonly TelegramBotClient _client;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IINNService _innService;
        public TgBot(string apiKey, IINNService iinService)
        {
            _tgApiKey = apiKey;
            _cancellationTokenSource = new CancellationTokenSource();
            _client = new TelegramBotClient(_tgApiKey, cancellationToken: _cancellationTokenSource.Token);
            _client.OnMessage += OnMessageRecieved;
            _client.OnError += OnError;
            _innService = iinService;
        }

        private async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception.Message);
        }

        public async Task<bool> Check()
        {
            return await _client.TestApi();
        }
        public async void Stop()
        {
            foreach (var id in _innRequests)
            {
                await SendCustomMessage(new MessageInfo(id, "Bot is shutting down. Request for INN declined"));
            }
            _cancellationTokenSource.Cancel();
        }

        private async Task OnMessageRecieved(Message message, UpdateType type)
        {
            if (message.Text == null)
            {
                Console.WriteLine($"[{message.Date}]: Received from {(message.From == null ? "Chat" : message.From.Username)}: {message.Type}");
                await SendCustomMessage(GetReplyMessage(message.Chat, "Unknown"));
                await SendCustomMessage(GetReplyMessage(message.Chat, HELP_COMMAND));
                return;
            }
            Console.WriteLine($"[{message.Date}]: Received from {(message.From == null ? "Chat" : message.From.Username)}: {message.Text}");
            if (_innRequests.Contains(message.Chat.Id))
            {
                await HandleINNRecieve(message);
                return;
            }
            await SendCustomMessage(GetReplyMessage(message.Chat, message.Text));
            if (message.Text.Equals(INN_COMMAND))
            {
                _innRequests.Add(message.Chat.Id);
            }

        }
        private async Task<Message> SendCustomMessage(MessageInfo info)
        {
            var res = await _client.SendMessage(info.ChatId, info.Text, info.ParseMode, replyMarkup: info.ReplyMarkup);
            Console.WriteLine($"[{res.Date}]: Sent to {res.Chat.Username}: {res.Text}");
            return res;
        }
        private static MessageInfo GetReplyMessage(Chat chat, string command)
        =>
            command switch
            {
                INIT_COMMAND => new MessageInfo(chat, WELCOME_MESSAGE),
                CREATOR_INFO_COMMAND => new MessageInfo(chat, CREATOR_INFO, ReplyMarkup: CREDENTIALS),
                HELP_COMMAND => new MessageInfo(chat, HELP_INFO, ReplyMarkup: COMMANDS_LIST),
                INN_COMMAND => new MessageInfo(chat, INN_INFO),
                _ => new MessageInfo(chat, "Unknown command")
            };
        private async Task HandleINNRecieve(Message message)
        {
            if (message.Text == null) return;
            _innRequests.Remove(message.Chat.Id);
            var res = await _innService.GetCompanyInfos(message.Text.Split(' '));
            var formattedResponse = string.Join("\n", res.Select(c => $"<b>{c.INN}</b>: {c.Name}, {c.Address}"));
            await SendCustomMessage(new MessageInfo(message.Chat, formattedResponse));
        }
    }
}
