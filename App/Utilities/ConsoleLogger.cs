namespace Bot.Utilities
{
    internal class ConsoleLogger
    {
        private readonly string _location;
        public ConsoleLogger(string location)
        {
            _location = location;
        }

        public static void BotApiLogger(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        //Local console logging
        public void BotActions(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()} BOT: - {_location} - {message}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void BackupAction(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()} BACKUP: - {_location} - {message}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        internal void ActionSucceed(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()} BACKUP: - {_location} - {message}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void Exception(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"<!!>{_location}: " + ex.Message);
            Console.WriteLine("=================================== Exception:\n" + ex.StackTrace);

            if (ex.InnerException is not null)
                Console.WriteLine("=================================== Inner:\n" + ex.InnerException);

            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
