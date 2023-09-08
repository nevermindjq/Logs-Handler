using Core.Models.Abstractions;

namespace Core.LogParsers;

public class RedlineFactory : ILogsFactory
{
    public async IAsyncEnumerable<ILog> CreateMany(string path)
    {
        foreach (var dir in Directory.GetDirectories(path))
        {
            yield return await Create(dir);
        }
    }
    
    public Task<ILog> Create(string path)
    {
        return Task.FromResult<ILog>(new Log
        {
            Path = path,
            HasTelegram = TelegramExists(path),
            HasSteam = SteamExists(path),
            HasWallets = WalletsExists(path),
            Passwords = ParsePasswords(Path.Combine(path, "Passwords.txt"))
        });
    }

    public async IAsyncEnumerable<IPasswordField> ParsePasswords(string path)
    {
        if (!File.Exists(path))
        {
            yield break;
        }
        
        var reader = new StreamReader(path);

        while (!reader.EndOfStream)
        {
            var url = await reader.ReadLineAsync();

            if (url == null || !url.StartsWith("URL"))
            {
                continue;
            }

            var username = await reader.ReadLineAsync();
            var password = await reader.ReadLineAsync();
            var application = await reader.ReadLineAsync();
            
            yield return new PasswordField
            {
                Url = url[(url.IndexOf(' ')+1)..],
                UserName = username![(username.IndexOf(' ')+1)..],
                Password = password![(password.IndexOf(' ')+1)..],
                Application = application![(application.IndexOf(' ')+1)..],
            };
        }
        
        reader.Dispose();
    }

    public bool TelegramExists(string path) => Directory.Exists(Path.Combine(path, "Telegram"));

    public bool SteamExists(string path) => Directory.Exists(Path.Combine(path, "Steam"));

    public bool WalletsExists(string path) => Directory.Exists(Path.Combine(path, "Wallets"));
}