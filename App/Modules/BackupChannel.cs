using App.Services;
using App.Services.Repository;
using Discord;
using Discord.WebSocket;

namespace App.Modules
{
    internal class BackupChannel
    {
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

            var backup = new BackupBuilder(command.Channel, command.User);

            while (true)
            {
                var messageBatch = await MakeBackup(command.Channel);

                foreach (var message in messageBatch)
                {
                    if (message == null) break;

                    if (MessageRepository.CheckIfExists(message.Id))
                    {
                        break;
                    }

                    backup.AddMessage(message);

                }
                //add message to db
                backup.Save();


            }
            await command.RespondAsync("oi"); //TODO IMPORTANT: Implement proper response
        }



        private async Task<IEnumerable<IMessage>> MakeBackup(ISocketMessageChannel channel) //Batch maker
        {
            var messages = await channel.GetMessagesAsync(50).FlattenAsync();

            return messages;
        }
    }
}
