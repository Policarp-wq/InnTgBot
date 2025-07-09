using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace InnTgBot.Bot
{
    public record MessageInfo(ChatId ChatId, string Text, ReplyMarkup? ReplyMarkup = null, ParseMode ParseMode = ParseMode.Html);
}
