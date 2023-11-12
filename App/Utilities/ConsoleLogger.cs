namespace Bot.Utilities
{
    internal class ConsoleLogger
    {
        private readonly string _location;
        public ConsoleLogger(string location)
        {
            _location = location;
        }

        //Methods with the intent to be used as a delegate argument for standard console logging
        public static void DBContextLogger(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        public static void BotLogger(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        //Static generic console logging
        public static void GenericBackupAction(string? location, string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()} - {location}: {message}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void GenericError(string? location, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()} - {location}: {message}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        //Non generic console logging
        public void BotActions(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"BOT: {DateTime.Now.ToLongTimeString()} - {_location}: {message}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void BackupAction(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Backup: {DateTime.Now.ToLongTimeString()} - {_location}: {message}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        internal void ActionSucceed(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Backup: {DateTime.Now.ToLongTimeString()} - {_location}: {message}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void Exception(string operation, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"<!!> {operation}");
            Console.WriteLine($"{_location}: " + ex.Message);
            Console.WriteLine("=================================== Exception:\n" + ex.StackTrace);

            if (ex.InnerException is not null)
                Console.WriteLine("=================================== Inner:\n" + ex.InnerException);

            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
