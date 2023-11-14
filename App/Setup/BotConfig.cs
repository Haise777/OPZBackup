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
        public ulong StarRoleId { get; private set; }
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
                throw new FileNotFoundException();
            }

            using var sr = new StreamReader(_filePath);

            var lines = sr.ReadToEnd().Split(Environment.NewLine);
            var values = ParseAllValues(lines);

            Token = values.tkn.Length > 1 ? values.tkn :
                throw new Exception("invalid TOKEN value");
            GuildId = values.gId > 100000 ? values.gId :
                throw new Exception("invalid GUILD_ID value");
            StarRoleId = values.sRole > 100000 ? values.sRole :
                throw new Exception("invalid STARROLE_ID value");
            ConnectionString = values.cString.Length > 1 ? values.cString :
                throw new Exception("invalid CONNECTION_STRING value");

        }

        private (string tkn, ulong gId, ulong sRole, string cString) ParseAllValues(string[] values)
        {
            try
            {
                string token = values[0].Trim().Substring(6);
                ulong guildId = ulong.Parse(values[1].Trim().Substring(9));
                ulong starRole = ulong.Parse(values[2].Trim().Substring(12));
                string connectionString = values[3].Trim().Substring(18);

                return (token, guildId, starRole, connectionString);
            }
            catch (Exception ex)
            {
                throw new Exception("Configuration file is corrupted, consider reconfiguring or deleting it", ex);
            }
        }

        private void CreateConfigFile()
        {
            _logger.BotActions("Creating configurable file, consider configuring it before initiating bot again");
            using var sw = new StreamWriter(_filePath);

            sw.WriteLine("TOKEN=");
            sw.WriteLine("GUILD_ID=");
            sw.WriteLine("STARROLE_ID=");
            sw.WriteLine("CONNECTION_STRING=");
        }
    }
}
