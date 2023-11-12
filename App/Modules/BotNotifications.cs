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

        public async Task SendBackupCompletedMessage(BackupRegister backupRegister, ulong otherBackupStartId = 1)
        {
            var backupCompletedEmbed = await BackupCompletedMessage(backupRegister, otherBackupStartId);

            await _command.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = "blank";
                    msg.Embed = backupCompletedEmbed;
                }
            );
        }

        private async Task<Embed> BackupCompletedMessage(BackupRegister backupRegister, ulong otherBackupLastMessageId)
        {
            var author = Program.testGuild.GetUser(backupRegister.AuthorId.Value);
            var startMessage = await _command.Channel.GetMessageAsync(backupRegister.EndMessageId.Value);
            var lastMessage = await _command.Channel.GetMessageAsync(backupRegister.EndMessageId.Value);
            var startDate = backupRegister.Date;
            var endDate = DateTime.UtcNow;

            var startMessageField = new EmbedFieldBuilder()
                .WithName("De:")
                .WithValue($"{startMessage.Author.GlobalName} {startMessage.Timestamp}\n" +
                $"{startMessage.Content}")
                .WithIsInline(false);
            var endMessageField = new EmbedFieldBuilder()
                .WithName("Até:")
                .WithValue($"{lastMessage.Author.GlobalName} {lastMessage.Timestamp}\n" +
                $"{lastMessage.Content}")
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
                .AddField(startMessageField)
                .AddField(endMessageField)
                .AddField(startTime)
                .AddField(endTime)
                .WithFooter(madeBy);

            if (otherBackupLastMessageId != 1)
                embed.WithDescription("ate o ultimo backup LINK {GetMessageById(otherBackupLastMessageId)}");

            return embed.Build();
        }
    }
}
