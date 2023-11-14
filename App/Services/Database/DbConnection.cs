using Bot.Services.Database.Context;
using Bot.Utilities;

namespace Bot.Services.Database
{
    internal class DbConnection
    {
        private MessageBackupContext? _currentConnection;
        private ConsoleLogger _logger = new(nameof(DbConnection));

        public void OpenConnection()
        {
            if (_currentConnection is not null)
            {
                _logger.BackupAction("Connection is already open!");
                return;
            }
            _currentConnection = new MessageBackupContext();
            _logger.BackupAction("Connection opened");
        }

        public void CloseConnection()
        {
            if (_currentConnection is null)
            {
                _logger.BackupAction("There is no connection to close!");
                return;
            }

            _currentConnection.Dispose();
            _currentConnection = null;
            _logger.BackupAction("Connection closed");
        }

        public MessageBackupContext GetConnection()
        {
            if (_currentConnection is null)
                throw new InvalidOperationException("No available connection to get");
            return _currentConnection;
        }
    }
}
