using Bot.Services.Database.Models;
using Discord;
using Discord.WebSocket;

namespace Bot.Modules.BackupMessage
{
    internal class BackupNotification
    {
        public async Task SendMakingBackupMessage(SocketSlashCommand command)
        {
            await command.RespondAsync("a...");
        }

        public async Task SendBackupCompletedMessage(BackupRegister backupRegister, SocketSlashCommand command)
        {
            var backupCompletedEmbed = await BackupCompletedMessage(command, backupRegister);

            await command.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = "";
                    msg.Embed = backupCompletedEmbed;
                }
            );
            var completionPing = await command.Channel.SendMessageAsync($"<@{command.User.Id}>");
            await Task.Delay(1000);
            await command.Channel.DeleteMessageAsync(completionPing.Id);
        }

        internal async Task AlreadyExecutingBackup(SocketSlashCommand command)
        {
            var component = new ComponentBuilder().WithButton(label: "foda-se", style: ButtonStyle.Secondary, customId: "bola");
            await command.RespondAsync(
                "Há um backup sendo feito no momento, tente novamente mais tarde...", components: component.Build());

            await Task.Delay(7500);
            await command.DeleteOriginalResponseAsync();
        }

        //TODO: Fix message timestamp timezone being +3:00 ahead
        private async Task<Embed> BackupCompletedMessage(SocketSlashCommand command, BackupRegister backupRegister)
        {
            var backupAuthor = Program.Guild.GetUser(backupRegister.AuthorId.Value);

            var messageData = await FormatToEmbedData(command, backupRegister);


            var startMessageField = new EmbedFieldBuilder()
                .WithName("De:")
                .WithValue($"{messageData.startMessage.Author.Username} {messageData.sMsgDate}\n" +
                $"{messageData.startMessage.Content}")
                .WithIsInline(false);
            var endMessageField = new EmbedFieldBuilder()
                .WithName("Até:")
                .WithValue($"{messageData.endMessage.Author.Username} {messageData.eMsgDate}\n" +
                $"{messageData.endMessage.Content}")
                .WithIsInline(false);

            var startTime = new EmbedFieldBuilder()
                .WithName("Iniciado:")
                .WithValue(backupRegister.Date.ToLongTimeString())
                .WithIsInline(true);
            var endTime = new EmbedFieldBuilder()
                .WithName("Terminado:")
                .WithValue(DateTime.Now.ToLongTimeString())
                .WithIsInline(true);

            var madeBy = new EmbedFooterBuilder()
                .WithText(backupAuthor.Username)
                .WithIconUrl($"{backupAuthor.GetAvatarUrl()}");

            var embed = new EmbedBuilder()
                .WithTitle("Backup realizado!")
                .WithColor(Color.Green)
                .AddField(startMessageField)
                .AddField(endMessageField)
                .AddField(startTime)
                .AddField(endTime)
                .WithFooter(madeBy);

            return embed.Build();
        }

        private async Task<(IMessage startMessage, IMessage endMessage, string sMsgDate, string eMsgDate)>
            FormatToEmbedData(SocketSlashCommand command, BackupRegister backupRegister)
        {
            var startMessage = await command.Channel.GetMessageAsync(backupRegister.StartMessageId.Value);
            var endMessage = await command.Channel.GetMessageAsync(backupRegister.EndMessageId.Value);
            var sMsgDate =
                $"{startMessage.Timestamp.DateTime.ToShortDateString()} " +
                $"{startMessage.Timestamp.DateTime.ToShortTimeString()}";
            var eMsgDate =
                $"{endMessage.Timestamp.DateTime.ToShortDateString()} " +
                $"{endMessage.Timestamp.DateTime.ToShortTimeString()}";

            return (startMessage, endMessage, sMsgDate, eMsgDate);
        }

        internal async void NotAuthorized(SocketSlashCommand command)
        {
            await command.RespondAsync("Não tem permissões necessárias para utilizar este comando");

            await Task.Delay(6000);

            await command.DeleteOriginalResponseAsync();
        }
    }
}
