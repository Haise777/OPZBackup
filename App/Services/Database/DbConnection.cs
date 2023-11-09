﻿using App.Services.Database.Context;

namespace App.Services.Database
{
    internal static class DbConnection
    {
        private static MessageBackupContext? _currentConnection = null;

        public static void OpenConnection()
        {
            if (_currentConnection is not null)
            {
                ConsoleLogger.GenericError(nameof(DbConnection), "Connection is already open!");
                return;
            }
            _currentConnection = new MessageBackupContext();
            Console.WriteLine("DbConnection: Connection opened");
            ConsoleLogger.GenericBackupAction(nameof(DbConnection), "Connection opened");
        }

        public static void CloseConnection()
        {
            if (_currentConnection is null)
            {
                ConsoleLogger.GenericError(nameof(DbConnection), "There is no connection to close!");
                return;
            }
            _currentConnection.Dispose();
            _currentConnection = null;
            ConsoleLogger.GenericBackupAction(nameof(DbConnection), "Connection closed");
        }

        public static MessageBackupContext GetConnection()
        {
            if (_currentConnection is null)
                throw new InvalidOperationException("No available connection to get");
            return _currentConnection;
        }
    }
}