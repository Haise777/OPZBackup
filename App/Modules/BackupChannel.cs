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
                    if (fazerCommandOptions.Name == "total")
                    {
                        await Backup(command);

                    }
                    else if (fazerCommandOptions.Name == "")
                    {

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
            ulong startFrom = 1;
            bool EndOfChannel = false;

            while (!EndOfChannel)
            {
                var messageBatch = await GetMessages(command.Channel, startFrom);

                foreach (var message in messageBatch)
                {
                    if (message == null)
                    {
                        EndOfChannel = true;
                        break;
                    }

                    if (MessageRepository.CheckIfExists(message.Id))
                    {
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
            IEnumerable<IMessage> messages;

            if (startFrom != 1)
                messages = await channel.GetMessagesAsync().FlattenAsync();
            else
                messages = await channel.GetMessagesAsync(startFrom, Direction.Before).FlattenAsync();

            return messages;
        }
    }
}
