using App.Services.Database.Models;
using Discord;
using Discord.WebSocket;

namespace App.Modules
{
    internal class BotNotifications
    {
        private readonly SocketSlashCommand _command;

        public BotNotifications(SocketSlashCommand command)
        {
            _command = command;
        }

        public async Task SendMakingBackupMessage()
        {
            await _command.RespondAsync("a...");
        }

        public async Task SendBackupCompletedMessage(BackupRegister backupRegister, ulong otherBackupStartId = 0)
        {
            var backupCompletedEmbed = await BackupCompletedMessage(backupRegister, otherBackupStartId);

            await _command.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = "1";
                    msg.Embed = backupCompletedEmbed;
                }
            );
        }



        private async Task<Embed> BackupCompletedMessage(BackupRegister backupRegister, ulong otherBackupLastMessageId)
        {
            var author = Program.testGuild.GetUser(backupRegister.AuthorId ?? throw new Exception());
            var startDate = backupRegister.Date;
            var endDate = DateTime.UtcNow;
            var lastMessagedsa = await _command.Channel.GetMessageAsync(backupRegister.EndMessageId ?? throw new Exception());
            var startMessagedsa = await _command.Channel.GetMessageAsync(backupRegister.EndMessageId ?? throw new Exception());

            var startMessage = new EmbedFieldBuilder()
                .WithName("De:")
                .WithValue($"{startMessagedsa.Author.GlobalName} {startMessagedsa.Timestamp}\n" +
                $"{startMessagedsa.Content}")
                .WithIsInline(false);
            var endMessage = new EmbedFieldBuilder()
                .WithName("Até:")
                .WithValue($"{lastMessagedsa.Author.GlobalName} {lastMessagedsa.Timestamp}\n" +
                $"{lastMessagedsa.Content}")
                .WithIsInline(false);

            var startTime = new EmbedFieldBuilder()
                .WithName("Iniciado:")
                .WithValue($"{backupRegister.Date}")
                .WithIsInline(true);
            var endTime = new EmbedFieldBuilder()
                .WithName("Terminado:")
                .WithValue($"{DateTime.Now}")
                .WithIsInline(true);

            var madeBy = new EmbedFooterBuilder()
                .WithText($"Feito por: {author.GlobalName}")
                .WithIconUrl($"{author.GetAvatarUrl}");

            var embed = new EmbedBuilder()
                .WithTitle("Backup realizado com sucesso!")
                .WithColor(Color.Green)
                .AddField(startMessage)
                .AddField(endMessage)
                .AddField(startTime)
                .AddField(endTime)
                .WithFooter(madeBy);

            if (otherBackupLastMessageId != 0)
                embed.WithDescription("ate o ultimo backup LINK {GetMessageById(otherBackupLastMessageId)}");

            return embed.Build();
        }
    }
}
