using App.Services.Database;
using App.Services.Database.Repository;
using App.Utilities;
using Discord;
using Discord.WebSocket;

namespace App.Modules
{
    internal class BackupCommand
    {
        private readonly ConsoleLogger _log = new(nameof(BackupCommand));
        private readonly BotNotifications _notifications;
        private readonly SocketSlashCommand _command;

        public BackupCommand(SocketSlashCommand command)
        {
            _command = command;
            _notifications = new BotNotifications(command);
        }

        public async Task BackupOptions()
        {
            var firstCommandOption = _command.Data.Options.First();
            var fazerCommandOptions = _command.Data.Options.First().Options.First();

            switch (firstCommandOption.Name)
            {
                //TODO: A way to block another backup command call when one is currently in execution

                case "fazer":
                    if (fazerCommandOptions.Name == "tudo" && (bool)fazerCommandOptions.Options.First().Value)
                    {
                        _log.BotActions(firstCommandOption.Name);
                        await _notifications.SendMakingBackupMessage();
                        await MakeBackup(false);
                    }
                    else if (fazerCommandOptions.Name == "ate-ultimo" && (bool)fazerCommandOptions.Options.First().Value)
                    {
                        _log.BotActions(firstCommandOption.Name);
                        await _notifications.SendMakingBackupMessage();
                        await MakeBackup(true);
                    }
                    else
                    {
                        throw new Exception("Invalid backup command option");
                    }
                    break;

                case "deletar":

                    if (fazerCommandOptions.Name == "proprio" && (bool)fazerCommandOptions.Options.First().Value)
                    {
                        await DeleteUserRecord(_command.User);
                    }
                    break;

                default:
                    throw new ArgumentException("Erro grave no BackupOptions SwitchCase");
            }
        }

        private async Task DeleteUserRecord(IUser author)
        {
            DbConnection.OpenConnection();
            AuthorRepository.DeleteAuthor(author); //TODO: To make awaitable
            DbConnection.CloseConnection();
        }

        private async Task MakeBackup(bool untilLastBackup)
        {
            //TODO IMPORTANT: Make a way to validate if backup should be made

            var backup = new Backup(_command.Channel, _command.User);
            var backupRegister = backup.BackupRegister;
            var shouldContinue = true;

            ulong startFrom = 1;
            while (shouldContinue)
            {
                var messageBatch = await GetMessages(_command.Channel, startFrom);

                if (!messageBatch.Any())
                {
                    await _notifications.SendBackupCompletedMessage(backupRegister);
                    _log.HappyAction("Reached end of channel, considering backup as finished");
                    break;
                }
                DbConnection.OpenConnection();
                foreach (var message in messageBatch)
                {
                    if (message == null)
                        throw new InvalidOperationException("Message object cannot be null");

                    //Checks if message already exists on database
                    if (MessageRepository.CheckIfExists(message.Id))
                    {
                        _log.BackupAction($"Already saved message found: '{message.Content}'");
                        if (untilLastBackup)
                        {
                            await _notifications.SendBackupCompletedMessage(backupRegister, message.Id);
                            _log.HappyAction("Reached already saved message, considering backup as finished");
                            shouldContinue = false;
                            break;
                        }
                        continue;
                    }

                    startFrom = message.Id;
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
                finally
                {
                    DbConnection.CloseConnection();
                }
            }
        }

        private async Task<IEnumerable<IMessage>> GetMessages(ISocketMessageChannel channel, ulong startFrom) //Batch maker
        {
            _log.BackupAction($"Getting messages from {channel.Name}");

            if (startFrom == 1)
            {
                _log.BackupAction("Starting from beginning");
                return await channel.GetMessagesAsync(8).FlattenAsync();
            }
            else
            {
                _log.BackupAction("Starting from older message");
                return await channel.GetMessagesAsync(startFrom, Direction.Before, 8).FlattenAsync();
            }
        }
    }
}
