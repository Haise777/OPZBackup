using Bot.Utilities;

namespace Bot
{
    internal class BotConfig
    {
        private static BotConfig? _instance;
        private readonly ConsoleLogger _logger = new(nameof(BotConfig));
        private readonly string _filePath;

        public ulong GuildId { get; private set; }
        public string Token { get; private set; }
        public ulong StarRole { get; private set; }
        public string ConnectionString { get; private set; }

        private BotConfig(string filepath)
        {
            _filePath = filepath;
            ReadConfig();
        }

        public static BotConfig GetBotConfigurations(string filepath)
        {
            if (_instance is not null)
                throw new InvalidOperationException("A Bot Configuration has already been instantiated");

            _instance = new BotConfig(filepath);
            return _instance;
        }

        private void ReadConfig()
        {
            _logger.BotActions("Reading config file");

            if (!File.Exists(_filePath))
            {
                CreateConfigFile();
                return;
            }

            using var sr = new StreamReader(_filePath);

            var lines = sr.ReadToEnd().Split(Environment.NewLine).ToList();

            ulong guildId = ulong.Parse(lines.Find(gId => gId.Trim().StartsWith("GUILD_ID=")).Substring(9));
            string token = lines.Find(t => t.Trim().StartsWith("TOKEN=")).Substring(6);
            ulong starRole = ulong.Parse(lines.Find(sR => sR.Trim().StartsWith("STARROLE_ID=")).Substring(12));
            string connectionString = lines.Find(cS => cS.Trim().StartsWith("CONNECTION_STRING=")).Substring(18);

            GuildId = guildId > 100000 ? guildId : throw new Exception("invalid GUILD_ID value");
            Token = token.Length > 1 ? token : throw new Exception("invalid TOKEN value");
            StarRole = starRole > 100000 ? starRole : throw new Exception("invalid STARROLE_ID value");
            ConnectionString = connectionString.Length > 1 ? connectionString : throw new Exception("invalid CONNECTION_STRING value");

        }

        private void CreateConfigFile()
        {
            _logger.BotActions("Config file not found, creating configurable one\n" +
                "         consider configuring it before initiating bot again");
            using var sw = new StreamWriter(_filePath);

            sw.WriteLine("GUILD_ID=");
            sw.WriteLine("TOKEN=");
            sw.WriteLine("STARROLE_ID=");
            sw.WriteLine("CONNECTION_STRING=");
        }
    }
}
