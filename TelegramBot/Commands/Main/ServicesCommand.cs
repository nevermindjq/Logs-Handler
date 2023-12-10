using TelegramBot.Models;
using TL;
using WTelegram;

namespace TelegramBot.Commands.Main;

public class ServicesCommand(Client client, Random random) : ICommand
{
    public bool AuthorizedOnly { get; } = true;

    public Task Invoke(UpdateNewMessage update, User user)
    {
        return client.Messages_SendMessage(
            user,
            "Available Services",
            random.NextInt64(),
            reply_markup: new ReplyKeyboardMarkup
            {
                rows = new []
                {
                    new KeyboardButtonRow
                    {
                        buttons = new []
                        {
                            new KeyboardButton
                            {
                                text = "Twitch"
                            }
                        }
                    },
                    new KeyboardButtonRow
                    {
                        buttons = new []
                        {
                            new KeyboardButton
                            {
                                text = "Back"
                            }
                        }
                    }
                },
                flags = ReplyKeyboardMarkup.Flags.resize
            });
    }
}