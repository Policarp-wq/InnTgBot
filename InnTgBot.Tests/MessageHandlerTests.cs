using InnTgBot.Bot;
using InnTgBot.Services;
using Moq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace InnTgBot.Tests
{
    public class MessageHandlerTests
    {
        [Fact]
        public async Task ReturnsHelpInlineButtons()
        {
            var innServiceMock = new Mock<IINNService>();
            var handler = new BotMessageHandler(innServiceMock.Object);
            var ans = await GetAnswerOnCommand(handler, BotMessageHandler.HELP_COMMAND);
            Assert.Equal(BotMessageHandler.COMMANDS_LIST.Length, (ans.ReplyMarkup as ReplyKeyboardMarkup).Keyboard.First().Count());
        }
        [Theory]
        [InlineData(BotMessageHandler.INIT_COMMAND, BotMessageHandler.WELCOME_MESSAGE)]
        [InlineData(BotMessageHandler.CREATOR_INFO_COMMAND, BotMessageHandler.CREATOR_INFO)]
        [InlineData(BotMessageHandler.INN_QUERY_COMMAND, BotMessageHandler.INN_REQUEST_INFO)]
        [InlineData("Some unknow", BotMessageHandler.UNKNOW_COMMAND_RESPONSE)]
        public async Task CommandResponseCorrect(string command, string expectedResponse)
        {
            var innServiceMock = new Mock<IINNService>();
            var handler = new BotMessageHandler(innServiceMock.Object);
            var ans = await GetAnswerOnCommand(handler, command);
            Assert.Equal(expectedResponse, ans.Text);
        }
        public static async Task<MessageInfo> GetAnswerOnCommand(BotMessageHandler handler, string command)
        {
            return await handler.GetAnswerMessage(new Message() { Chat = new Chat() { Id = 1 }, Text = command });
        }
    }
}
