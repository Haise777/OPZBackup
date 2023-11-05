using Discord;
using Discord.WebSocket;

namespace App.Modules
{
    internal class BackupChannel
    {


        public async Task Backup(SocketSlashCommand command)
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
