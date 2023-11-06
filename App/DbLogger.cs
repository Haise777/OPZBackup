namespace App
{
    internal class DbLogger
    {
        public static void LogContext(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }


    }
}
