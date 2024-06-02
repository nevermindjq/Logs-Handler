namespace Bot.Commands.Markups;

public static class Markup_User {
    public static KeyboardButtonRow[] StartMarkupRows { get; } = [
        new()
        {
            buttons =
            [
                new KeyboardButtonCallback()
                {
                    text = "Instagram",
                    data = "user_instagram".Utf8()
                },
                new KeyboardButtonCallback()
                {
                    text = "Twitter",
                    data = "user_twitter".Utf8() // TODO create callback
                }
            ]
        },
        new()
        {
            buttons =
            [
                new KeyboardButtonCallback()
                {
                    text = "Twitch",
                    data = "user_twitch".Utf8() // TODO create callback
                },
                new KeyboardButtonCallback()
                {
                    text = "Telegram",
                    data = "user_telegram".Utf8() // TODO create callback
                }
            ]
        },
        new()
        {
            buttons =
            [
                new KeyboardButtonCallback()
                {
                    text = "TikTok",
                    data = "user_tiktok".Utf8() // TODO create callback
                },
                new KeyboardButtonCallback()
                {
                    text = "YouTube",
                    data = "user_youtube".Utf8() // TODO create callback
                }
            ]
        }
    ]; 
    
    public static ReplyInlineMarkup StartMarkup { get; } = new()
    {
        rows = StartMarkupRows
    };
}