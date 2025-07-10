using InnTgBot.Services;
using System.Collections.Concurrent;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace InnTgBot.Bot
{
    public sealed class BotMessageHandler
    {
 
        public const string INIT_COMMAND = "/start";
        public const string CREATOR_INFO_COMMAND = "/hello";
        public const string HELP_COMMAND = "/help";
        public const string INN_QUERY_COMMAND = "/inn";
        public const string LAST_COMMAND = "/last";

        public readonly static string[] COMMANDS_LIST =
        [
                    CREATOR_INFO_COMMAND,
                    HELP_COMMAND,
                    INN_QUERY_COMMAND,
                    LAST_COMMAND,
        ];

        public const string WELCOME_MESSAGE = $"Hello! This bot can give you information about companies by their INN(ru abbr).\nType {HELP_COMMAND} for more info";
        public const string CREATOR_INFO = "My creator is Eugene Tatarkin @Policarp228. Useful links:";
        public readonly static string HELP_INFO = "Available commands:\n" + string.Join('\n', COMMANDS_LIST);
        public const string INN_REQUEST_INFO = "Send one or more INN numbers separated by spaces e.g.\n7709439560 7707083893 7709439560";
        public const string UNKNOW_COMMAND_RESPONSE = $"Unknown command. Type {HELP_COMMAND} for list of available commands";
        private static readonly InlineKeyboardButton[] CREDENTIALS =
                [
                    InlineKeyboardButton.WithUrl("GitHub", "https://github.com/Policarp-wq"),
                    InlineKeyboardButton.WithUrl("hh.ru", "https://hh.ru/resume/17c21b2fff0e996d490039ed1f454f75446353"),
                    InlineKeyboardButton.WithCopyText("email", "tatarkin-evgeniy@mail.ru")
                ];
        private readonly HashSet<long> _innRequests; // не потокобезопасен
        private readonly ConcurrentDictionary<long, string> _lastCommands;

        private readonly IINNService _innService;

        public BotMessageHandler(IINNService innService)
        {
            _innService = innService;
            _innRequests = new HashSet<long>();
            _lastCommands = new ConcurrentDictionary<long, string>();
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
                return new MessageInfo(chat, "Bot accepts only text messages. Sry ;(");
            if (_innRequests.Contains(chat.Id))
            {
                return await HandleINNRecieve(message);
            }
            if (command == LAST_COMMAND)
            {
                if (_lastCommands.TryGetValue(chat.Id, out string? value))
                {
                    command = value;
                }
                else return new MessageInfo(chat, "No last commands");
            }
            else
            {
                if(!_lastCommands.TryAdd(chat.Id, command))
                    _lastCommands[chat.Id] = command;
            }
            if (command == INN_QUERY_COMMAND)
            {
                _innRequests.Add(chat.Id);
                return new MessageInfo(chat, INN_REQUEST_INFO);
            }
            
            return command switch
            {
                INIT_COMMAND => new MessageInfo(chat, WELCOME_MESSAGE),
                CREATOR_INFO_COMMAND => new MessageInfo(chat, CREATOR_INFO, ReplyMarkup: CREDENTIALS),
                HELP_COMMAND => new MessageInfo(chat, HELP_INFO, ReplyMarkup: COMMANDS_LIST),
                _ => new MessageInfo(chat, UNKNOW_COMMAND_RESPONSE)
            };
        }
        private async Task<MessageInfo> HandleINNRecieve(Message message)
        {
            if (message.Text == null) throw new InvalidOperationException("Message text was null");
            _innRequests.Remove(message.Chat.Id);
            if (COMMANDS_LIST.Contains(message.Text) || INIT_COMMAND.Equals(message.Text))
            {
                return await GetAnswerMessage(message);
            }
            var res = await _innService.GetCompanyInfos(message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var formattedFoundResponse = string.Join("\n", res
                .Where(r => r.Info != null)
                .OrderBy(r => r.Info!.Name)
                .Select(r => $"<b>{r.Info!.INN}</b>: {r.Info.Name}, {r.Info.Address}")
                );
            var formattedBadResponse = string.Join("\n", res.Where(r => r.ErrorMessage != null).Select(r => $"<b>{r.Inn}</b>: {r.ErrorMessage}"));
            var response = formattedFoundResponse + (formattedBadResponse.Length > 0 ? "\nFailed:\n" + formattedBadResponse : "");
            return new MessageInfo(message.Chat, response);
        }

    }
}
