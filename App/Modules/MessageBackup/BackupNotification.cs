using Bot.Services.Database.Models;
using Discord;
using Discord.WebSocket;

namespace Bot.Modules.BackupMessage
{
    internal class BackupNotification
    {
        private SocketSlashCommand _command;

        public async Task SendMakingBackupMessage(SocketSlashCommand command)
        {
            _command = command;
            await command.RespondAsync("a...");
        }

        public async Task SendBackupCompletedMessage(BackupRegister backupRegister)
        {
            var backupCompletedEmbed = await BackupCompletedMessage(backupRegister);

            await _command.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = "";
                    msg.Embed = backupCompletedEmbed;
                }
            );
            var completionPing = await _command.Channel.SendMessageAsync($"<@{_command.User.Id}>");
            await Task.Delay(1000);
            await _command.Channel.DeleteMessageAsync(completionPing.Id);
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
        private async Task<Embed> BackupCompletedMessage(BackupRegister backupRegister)
        {
            var backupAuthor = Program.Guild.GetUser(backupRegister.AuthorId.Value);

            var messageData = await FormatToEmbedData(backupRegister);


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
            FormatToEmbedData(BackupRegister backupRegister)
        {
            var startMessage = await _command.Channel.GetMessageAsync(backupRegister.StartMessageId.Value);
            var endMessage = await _command.Channel.GetMessageAsync(backupRegister.EndMessageId.Value);
            var sMsgDate =
                $"{startMessage.Timestamp.DateTime.ToShortDateString()} " +
                $"{startMessage.Timestamp.DateTime.ToShortTimeString()}";
            var eMsgDate =
                $"{endMessage.Timestamp.DateTime.ToShortDateString()} " +
                $"{endMessage.Timestamp.DateTime.ToShortTimeString()}";

            return (startMessage, endMessage, sMsgDate, eMsgDate);
        }

        internal async Task NotAuthorized(SocketSlashCommand command)
        {
            await command.RespondAsync("Não tem permissões necessárias para utilizar este comando");

            await Task.Delay(6000);

            await command.DeleteOriginalResponseAsync();
        }

        internal async Task SendDeletingUserNotif(SocketSlashCommand? command)
        {
            await command.RespondAsync("Deletando usuario de todos os backups");
        }

        internal async Task UserDeletedNotif(SocketSlashCommand? command, bool wasDeleted = true)
        {
            if (!wasDeleted)
            {
                await command.ModifyOriginalResponseAsync(m => m.Content =
                $"Não há mensagens no backup pertencente à <@{command.User.Id}>");
                return;
            }

            await command.ModifyOriginalResponseAsync(m => m.Content =
            $"Todas as mensagens de <@{command.User.Id}> foram deletadas");
        }
    }
}
