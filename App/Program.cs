using Bot.Modules.BackupMessage;
using Bot.Services;
using Bot.Services.Database;
using Bot.Services.Database.Repository;
using Bot.Setup;
using Bot.Utilities;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bot
{
    internal class Program
    {
        static Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddTransient<Program>()
                        .AddScoped<BackupCommand, BackupCommand>()
                        .AddScoped<BackupNotification, BackupNotification>()
                        .AddScoped<BackupService, BackupService>()
                        .AddScoped<AuthorRepository, AuthorRepository>()
                        .AddScoped<BackupRegisterRepository, BackupRegisterRepository>()
                        .AddScoped<ChannelRepository, ChannelRepository>()
                        .AddScoped<MessageRepository, MessageRepository>()
                        .AddSingleton<DbConnection, DbConnection>();
                }).Build();
            var MainApp = host.Services.GetRequiredService<Program>();

            return MainApp.MainAsync();
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordSocketClient _client;
        private readonly ConsoleLogger _logger = new("Program");
        public static SocketGuild Guild { get; set; }
        public static ulong BotUserId { get; private set; }
        public static string DbConnectionString { get; private set; }

        public Program(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All
            };
            _client = new DiscordSocketClient(config);
        }

        public async Task MainAsync()
        {
            BotConfig botValues = null;
            try
            {
                botValues = BotConfig.GetBotConfigurations("bot.config");
            }
            catch (Exception ex)
            {
                _logger.Exception(ex);
                Environment.Exit(-1);
            }
            DbConnectionString = botValues.ConnectionString;
            AuthenticatorService.SetStarRole(botValues.StarRoleId);
            var handlers = new BotHandlers(_serviceProvider.GetRequiredService<IServiceScopeFactory>(), _logger);
            var readySetup = new BotClientReady(_client, botValues.GuildId);

            _client.Log += BotLog;
            _client.Ready += readySetup.Client_Ready;
            _client.SlashCommandExecuted += handlers.SlashCommandHandler;
            _client.ButtonExecuted += handlers.MyButtonHandler;

            await _client.LoginAsync(TokenType.Bot, botValues.Token);
            BotUserId = (await _client.GetApplicationInfoAsync()).Id;

            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private Task BotLog(LogMessage msg)
        {
            ConsoleLogger.BotApiLogger(msg.ToString());
            return Task.CompletedTask;
        }
    }
}