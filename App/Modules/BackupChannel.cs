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


                    }
                    else if (fazerCommandOptions.Name == "tudo")
                    {

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
            ulong theLastMessage = 1170601414171570186; //TODO Warning: Delete when possible
            var curChannel = command.Channel;
            string responseMessage = "";
            var messages = await curChannel.GetMessagesAsync(10).FlattenAsync();

            foreach (var message in messages)
            {
                if (message != null)
                {
                    if (message.Id != (ulong)theLastMessage)
                    {
                        responseMessage += $"{message.Content} -\n";

                    }
                    else
                    {
                        break;
                    }
                }
            }

            await command.RespondAsync(responseMessage);
        }
    }
}
