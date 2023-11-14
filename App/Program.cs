using Bot.Modules.BackupMessage;
using Bot.Services.Database;
using Bot.Services.Database.Repository;
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
        private DiscordSocketClient _client;
        private readonly ConsoleLogger _logger = new("Program");
        public static SocketGuild Guild;

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
                _logger.Exception("Failed to read bot configuration file", ex);
                Environment.Exit(-1);
            }
            var handlers = new BotHandlers(_serviceProvider.GetRequiredService<IServiceScopeFactory>(), _logger);
            Guild = _client.GetGuild(botValues.GuildId);
            var commands = new BotCommands(Guild);

            _client.Log += BotLog;
            _client.Ready += commands.Client_Ready;
            _client.SlashCommandExecuted += handlers.SlashCommandHandler;
            _client.ButtonExecuted += handlers.MyButtonHandler;

            await _client.LoginAsync(TokenType.Bot, botValues.Token);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private Task BotLog(LogMessage msg) //TODO Assign slash commands / Make it to not over declare already existing commands
        {
            ConsoleLogger.BotLogger(msg.ToString());
            return Task.CompletedTask;
        }
    }
}