using Bot.Services.Database.Models;
using Discord;
using Discord.WebSocket;

namespace Bot.Modules
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
            var backupCompletedEmbed = await BackupCompletedMessage(backupRegister);

            await _command.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = "";
                    msg.Embed = backupCompletedEmbed;
                }
            );
            var completionPing = await _command.Channel.SendMessageAsync($"<@{_command.User.Id}>");
            await _command.Channel.DeleteMessageAsync(completionPing.Id);
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

        //TODO: Fix message timestamp timezone being +3:00 ahead
        private async Task<Embed> BackupCompletedMessage(BackupRegister backupRegister)
        {
            var backupAuthor = Program.testGuild.GetUser(backupRegister.AuthorId.Value);

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
                .WithText($"Realizado por: {backupAuthor.Username}")
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

        internal async Task AlreadyExecutingBackup()
        {

            var component = new ComponentBuilder().WithButton(label: "foda-se", style: ButtonStyle.Secondary, customId: "bola");

            await _command.RespondAsync(
                "Há um backup sendo feito no momento, tente novamente mais tarde...", components: component.Build());
            await Task.Delay(5000);
            await _command.DeleteOriginalResponseAsync();
        }
    }
}
