using InnTgBot.Services;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace InnTgBot.Bot
{
    public class BotMessageHandler
    {
        public const string WELCOME_MESSAGE = "Hello! This bot can give you information about companies by ther INN(ru abbr)";
        public const string CREATOR_INFO = "My creator is Eugene Tatarkin. Useful links:";
        public const string HELP_INFO = "Available commands";
        public const string INN_INFO = "Send one or more INN numbers separated by spaces e.g. 7709439560 7707083893 7709439560";

        public const string INIT_COMMAND = "/start";
        public const string CREATOR_INFO_COMMAND = "/hello";
        public const string HELP_COMMAND = "/help";
        public const string INN_COMMAND = "/inn";
        public const string LAST_COMMAND = "/last";

        public static InlineKeyboardButton[] CREDENTIALS =
                [
                    InlineKeyboardButton.WithUrl("GitHub", "https://github.com/Policarp-wq"),
                    InlineKeyboardButton.WithUrl("hh.ru", "https://hh.ru/resume/17c21b2fff0e996d490039ed1f454f75446353"),
                    InlineKeyboardButton.WithCopyText("email", "tatarkin-evgeniy@mail.ru")
                ];
        private HashSet<long> _innRequests;
        private static string[] COMMANDS_LIST =
                [
                    HELP_COMMAND,
                    CREATOR_INFO_COMMAND,
                    INN_COMMAND,
                    LAST_COMMAND,
                ];
        private readonly IINNService _innService;

        public BotMessageHandler(IINNService innService)
        {
            _innService = innService;
            _innRequests = new HashSet<long>();
        }

        public List<long> ClearInnRequests()
        {
            var copy = _innRequests.ToList();
            _innRequests.Clear();
            return copy;
        }
        public async Task<MessageInfo> GetAnswerMessage(Message message)
        {
            var (command, chat) = (message.Text, message.Chat);
            if (command == null)
                return new MessageInfo(chat, "Bot accepts only text messages");
            if (_innRequests.Contains(chat.Id))
            {
                return await HandleINNRecieve(message);
            }
            if (command == INN_COMMAND)
            {
                _innRequests.Add(chat.Id);
                return new MessageInfo(chat, INN_INFO);
            }
            return command switch
            {
                INIT_COMMAND => new MessageInfo(chat, WELCOME_MESSAGE),
                CREATOR_INFO_COMMAND => new MessageInfo(chat, CREATOR_INFO, ReplyMarkup: CREDENTIALS),
                HELP_COMMAND => new MessageInfo(chat, HELP_INFO, ReplyMarkup: COMMANDS_LIST),
                _ => new MessageInfo(chat, "Unknown command")
            };
        }
        private async Task<MessageInfo> HandleINNRecieve(Message message)
        {
            if (message.Text == null) throw new InvalidOperationException("Message text was null");
            _innRequests.Remove(message.Chat.Id);
            var res = await _innService.GetCompanyInfos(message.Text.Split(' '));
            var formattedFoundResponse = string.Join("\n", res.Where(r => r.Info != null).Select(r => $"<b>{r.Info!.INN}</b>: {r.Info.Name}, {r.Info.Address}"));
            var formattedBadResponse = string.Join("\n", res.Where(r => r.ErrorMessage != null).Select(r => $"<b>{r.Inn}</b>: {r.ErrorMessage}"));
            var response = formattedFoundResponse + "\nFailed:\n" + formattedBadResponse;
            return new MessageInfo(message.Chat, response);
        }

    }
}
