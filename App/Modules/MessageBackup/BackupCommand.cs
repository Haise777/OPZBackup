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
                    {
                        await _notifications.NotAuthorized(command);
                        _log.BotActions($"{command.User.Username} dont have permission to use this command");
                        return;
                    }
                    if (_alreadyInExecution)
                    {
                        _log.BotActions("<!> Blocked backup attempt while another backup is already running");
                        await _notifications.AlreadyExecutingBackup(command);
                        return;
                    }
                    _alreadyInExecution = true;
                    await _notifications.SendMakingBackupMessage(_command);
                    if (commandChoice.Name == "tudo") await MakeBackup(false);
                    else if (commandChoice.Name == "ate-ultimo") await MakeBackup(true);
                    break;

                case "deletar":
                    if (commandChoice.Name == "proprio")
                        await DeleteUserRecord(_command.User);
                    break;

                default: throw new InvalidOperationException("Invalid backup command");
            }

            _alreadyInExecution = false;
        }

        private async Task DeleteUserRecord(IUser author)
        {
            _dbConnection.OpenConnection();
            await _notifications.SendDeletingUserNotif(_command);

            if (!_backupRepositoryAccess.CheckIfAuthorExists(author))
            {
                await _notifications.UserDeletedNotif(_command, false);
                _dbConnection.CloseConnection();
                return;
            }
            _backupRepositoryAccess.DeleteAuthor(author);
            _dbConnection.CloseConnection();
            await _notifications.UserDeletedNotif(_command);
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
                return await channel.GetMessagesAsync(8).FlattenAsync();
            else
                return await channel.GetMessagesAsync(startFrom, Direction.Before, 8).FlattenAsync();
        }
        //TODO: Make a way for backup to save already existing but edited messages
        private List<IMessage> FilterMessagesToSave(IEnumerable<IMessage> messages, bool onlyToLastBackup, out bool shouldContinue)
        {
            shouldContinue = true;
            var messagesToSave = new List<IMessage>();
            foreach (var message in messages)
            {
                if (message == null) throw new InvalidOperationException("Message object cannot be null");
                if (message.Author.Id == Program.BotUserId) continue;
                if (_messageRepository.CheckIfExists(message.Id))
                {
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
            finally
            {
                _dbConnection.CloseConnection();
            }
        }
    }
}
