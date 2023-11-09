namespace App
{
    internal class ConsoleLogger
    {
        private readonly string _location;
        public ConsoleLogger(string location)
        {
            _location = location;
        }

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

        // Actions
        public static void GenericBotActions(string? location, string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()} - {location}: {message}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

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

        public static void GenericException(string? location, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{location}: " + ex.Message);
            Console.WriteLine("=================================== Exception:\n" + ex.StackTrace);

            if (ex.InnerException != null)
                Console.WriteLine("=================================== Inner:\n" + ex.InnerException);

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void Exception(string operation, Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(operation);
            Console.WriteLine($"{_location}: " + ex.Message);
            Console.WriteLine("=================================== Exception:\n" + ex.StackTrace);

            if (ex.InnerException != null)
                Console.WriteLine("=================================== Inner:\n" + ex.InnerException);

            Console.ForegroundColor = ConsoleColor.Gray;
        }

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

        internal void HappyAction(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Backup: {DateTime.Now.ToLongTimeString()} - {_location}: {message}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
