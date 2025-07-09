using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace InnTgBot.Bot
{
    public class TgBot
    {
        public static string WELCOME_MESSAGE = "Hello! This bot can give you information about companies by ther INN(ru abbr)";
        public static string CREATOR_INFO = "My creator is Eugene Tatarkin<br> email: tatarkin-evgeniy@mail.ru, github: https://github.com/Policarp-wq, hh: ";
        //public static string HELP_INFO = "<ul><li>/start</li><li>/hello</li><li>/help</li></ul>";
        public static string HELP_INFO = "Trying <b>all the parameters</b> of <code>sendMessage</code> method";
        
        public const string INIT_COMMAND = "start";
        public const string CREATOR_INFO_COMMAND = "hello";
        public const string HELP_COMMAND = "help";
        private readonly string API_KEY;
        private readonly TelegramBotClient _client;
        private readonly CancellationTokenSource _cancellationTokenSource;
        public TgBot(string apiKey)
        {
            API_KEY = apiKey;
            _cancellationTokenSource = new CancellationTokenSource();    
            _client = new TelegramBotClient(API_KEY, cancellationToken: _cancellationTokenSource.Token);
            _client.OnMessage += OnMessageRecieved;
        }
        public async Task<bool> Check()
        {
            return await _client.TestApi();
        }
        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        private async Task OnMessageRecieved(Message message, UpdateType type)
        {
            if(message.Text == null)
                return;
            var chat = message.Chat;
            var res = await _client.SendMessage(chat, GetReplyMessage(message.Text), ParseMode.Html, replyMarkup: new InlineKeyboardButton[] 
            {
                InlineKeyboardButton.WithUrl("GitHub", "https://github.com/Policarp-wq"),
                InlineKeyboardButton.WithUrl("hh.ru", "https://hh.ru/resume/17c21b2fff0e996d490039ed1f454f75446353"),
                InlineKeyboardButton.WithCopyText("email", "tatarkin-evgeniy@mail.ru")
            });
            Console.WriteLine($"[{message.Date}]: Received from {(message.From == null ? "Chat" : message.From.Username)}: {message.Text}");
            Console.WriteLine($"[{res.Date}]: Sent to {(message.From == null ? "Chat" : message.From.Username)}: {res.Text}");
        }
        private string GetReplyMessage(string command)
        =>
            command switch
            {
                INIT_COMMAND => WELCOME_MESSAGE,
                CREATOR_INFO_COMMAND => CREATOR_INFO,
                HELP_COMMAND => HELP_INFO,
                _ => "Unknown command"
            };
        
    }
}
