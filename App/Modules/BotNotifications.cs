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

        private async Task<Embed> BackupCompletedMessage(BackupRegister backupRegister)
        {
            var author = Program.testGuild.GetUser(backupRegister.AuthorId.Value);
            var startMessage = await _command.Channel.GetMessageAsync(backupRegister.StartMessageId.Value);
            IMessage lastMessage = lastMessage = await _command.Channel.GetMessageAsync(backupRegister.EndMessageId.Value);

            var startMessageDate = $"{startMessage.Timestamp.DateTime.ToShortDateString()} {startMessage.Timestamp.DateTime.ToShortTimeString()}";
            var lastMessageDate = $"{lastMessage.Timestamp.DateTime.ToShortDateString()} {lastMessage.Timestamp.DateTime.ToShortTimeString()}";

            var startTime = new EmbedFieldBuilder()
                .WithName("Iniciado:")
                .WithValue(backupRegister.Date.ToLongTimeString())
                .WithIsInline(true);
            var endTime = new EmbedFieldBuilder()
                .WithName("Terminado:")
                .WithValue(DateTime.Now.ToLongTimeString())
                .WithIsInline(true);

            var startMessageField = new EmbedFieldBuilder()
                .WithName("De:")
                .WithValue($"{startMessage.Author.Username} {startMessageDate}\n" +
                $"{startMessage.Content}")
                .WithIsInline(false);
            var endMessageField = new EmbedFieldBuilder()
                .WithName("Até:")
                .WithValue($"{lastMessage.Author.Username} {lastMessageDate}\n" +
                $"{lastMessage.Content}")
                .WithIsInline(false);

            var madeBy = new EmbedFooterBuilder()
                .WithText($"Realizado por: {author.Username}")
                .WithIconUrl($"{author.GetAvatarUrl()}");

            var embed = new EmbedBuilder()
                .WithTitle("Backup realizado!")
                .WithColor(Color.Green)
                .AddField(startTime)
                .AddField(endTime)
                .AddField(startMessageField)
                .AddField(endMessageField)
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
