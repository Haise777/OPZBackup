using App.Services;
using App.Services.Repository;
using Discord;
using Discord.WebSocket;

namespace App.Modules
{
    internal class BackupChannel
    {
        private ConsoleLogger _log = new ConsoleLogger(nameof(BackupChannel));

        public async Task BackupOptions(SocketSlashCommand command)
        {
            int inputValue = 0;

            var firstCommandOption = command.Data.Options.First();
            var fazerCommandOptions = command.Data.Options.First().Options.First();

            switch (firstCommandOption.Name)
            {
                case "fazer":
                    if (((bool)fazerCommandOptions.Value))
                    {
                        _log.BotActions(firstCommandOption.Name);
                        await Backup(command);

                    }
                    else
                    {
                        await command.RespondAsync();
                    }
                    break;

                case "deletar":

                    if (fazerCommandOptions.Name == "proprio")
                    {

                    }
                    break;

                default:
                    throw new ArgumentException("Erro grave no BackupOptions SwitchCase");

            }
        }




        private async Task Backup(SocketSlashCommand command)
        {
            //TODO IMPORTANT: Make a way to validate if backup should be made

            var backup = new Backup(command.Channel, command.User);
            bool EndOfChannel = false;
            ulong startFrom = 1;

            while (!EndOfChannel)
            {
                var messageBatch = await GetMessages(command.Channel, startFrom);

                foreach (var message in messageBatch)
                {
                    //Check if it reached end of the channel
                    if (message == null)
                    {
                        _log.BackupAction("Null message value found, considering end of channel");
                        EndOfChannel = true;
                        break;
                    }

                    //If message already exists on database,
                    //jumps to the last saved message from that backup
                    if (MessageRepository.CheckIfExists(message.Id))
                    {
                        _log.BackupAction($"Already saved message found: {message.Content}\n" +
                            $"jumping to last backuped message");
                        startFrom = BackupRegisterRepository.GetOldestMessageId(message.Id);
                        break;
                    }

                    backup.AddMessage(message);
                }

                //add message to db
                try
                {
                    backup.Save();
                }
                catch (Exception ex)
                {
                    _log.Exception("Failed to save current backup batch", ex);
                }

            }
            await command.RespondAsync("oi"); //TODO IMPORTANT: Implement proper response
        }



        private async Task<IEnumerable<IMessage>> GetMessages(ISocketMessageChannel channel, ulong startFrom) //Batch maker
        {
            _log.BackupAction($"Getting messages from {channel.Name}");
            IEnumerable<IMessage> messages;

            if (startFrom != 1)
            {
                _log.BackupAction("Starting from beginning");
                messages = await channel.GetMessagesAsync(8).FlattenAsync();
            }
            else
            {
                _log.BackupAction("Starting from older message");
                messages = await channel.GetMessagesAsync(startFrom, Direction.Before, 8).FlattenAsync();
            }
            return messages;
        }
    }
}
