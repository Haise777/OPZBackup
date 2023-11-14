using Bot.Services;
using Bot.Services.Database;
using Bot.Services.Database.Repository;
using Bot.Utilities;
using Discord;
using Discord.WebSocket;

namespace Bot.Modules.BackupMessage
{
    internal class BackupCommand
    {
        private readonly ConsoleLogger _log;
        private readonly BackupService _backupRepositoryAccess;
        private readonly BackupNotification _notifications;
        private readonly DbConnection _dbConnection;
        private readonly MessageRepository _messageRepository;

        private static bool _alreadyInExecution;
        private SocketSlashCommand? _command;

        public BackupCommand(
            BackupService bService, BackupNotification bNotif, DbConnection dbConnection, MessageRepository msgRepository)
        {
            _log = new ConsoleLogger(nameof(BackupCommand));
            _backupRepositoryAccess = bService;
            _notifications = bNotif;
            _dbConnection = dbConnection;
            _messageRepository = msgRepository;
        }

        public async Task BackupOptions(SocketSlashCommand command)
        {
            _command = command;
            var commandAction = _command.Data.Options.First();
            var commandChoice = _command.Data.Options.First().Options.First();
            _log.BotActions($"{commandAction.Name} {commandChoice.Name}");

            switch (commandAction.Name)
            {
                case "fazer":
                    if (!AuthenticatorService.IsAuthorized(command.User))
                        _notifications.NotAuthorized(command);

                    if (_alreadyInExecution)
                    {
                        _log.BotActions("<!> Blocked backup attempt while another backup is already running");
                        await _notifications.AlreadyExecutingBackup(command);
                        return;
                    }
                    _alreadyInExecution = true;
                    await _notifications.SendMakingBackupMessage(_command);

                    if (commandChoice.Name == "tudo")
                        await MakeBackup(false);
                    else if (commandChoice.Name == "ate-ultimo")
                        await MakeBackup(true);
                    break;

                case "deletar":

                    if (commandChoice.Name == "proprio")
                        await DeleteUserRecord(_command.User);
                    break;

                default:
                    throw new InvalidOperationException("Invalid backup command");
            }

            _alreadyInExecution = false;
        }

        private async Task DeleteUserRecord(IUser author)
        {
            _dbConnection.OpenConnection();
            _backupRepositoryAccess.DeleteAuthor(author); //TODO: To make awaitable AND
            _dbConnection.CloseConnection();
        }

        private async Task MakeBackup(bool untilLastBackup)
        {
            //TODO IMPORTANT: Make a way to validate if backup should be made

            var backup = new Backup(_command.Channel, _command.User, _backupRepositoryAccess);
            var backupRegister = backup.BackupRegister;
            var shouldContinue = true;
            ulong startFromMessageId = 1;

            while (shouldContinue)
            {
                var messageBatch = await GetMessages(_command.Channel, startFromMessageId);
                if (!messageBatch.Any()) break;

                _dbConnection.OpenConnection();

                var messagesToSave = FilterMessagesToSave(messageBatch, untilLastBackup, out shouldContinue);

                startFromMessageId = messageBatch.Last().Id;
                if (!messagesToSave.Any())
                {
                    _dbConnection.CloseConnection();
                    continue;
                }

                backup.AddMessages(messagesToSave);
                SaveBatch(backup);
            }

            await _notifications.SendBackupCompletedMessage(backupRegister);
            _log.ActionSucceed("Reached end of channel, considering backup as finished");
        }

        private async Task<IEnumerable<IMessage>> GetMessages(ISocketMessageChannel channel, ulong startFrom)
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

        private List<IMessage> FilterMessagesToSave(IEnumerable<IMessage> messages, bool onlyToLastBackup, out bool shouldContinue)
        {
            shouldContinue = true;
            var messagesToSave = new List<IMessage>();
            foreach (var message in messages)
            {
                if (message == null)
                    throw new InvalidOperationException("Message object cannot be null");

                if (_messageRepository.CheckIfExists(message.Id))
                {
                    _log.BackupAction($"Already saved message found: '{message.Content}'");
                    if (onlyToLastBackup)
                    {
                        shouldContinue = false;
                        break;
                    }
                    continue;
                }

                messagesToSave.Add(message);
            }

            return messagesToSave;
        }

        private void SaveBatch(Backup backup)
        {
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
                _dbConnection.CloseConnection();
            }
        }
    }
}
