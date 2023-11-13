using Bot.Modules.BackupMessage;
using Bot.Services.Database;
using Bot.Services.Database.Repository;
using Bot.Utilities;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Bot
{
    internal class Program
    {
        public ulong testGuildId = ulong.Parse(File.ReadAllText(@"E:\archives\privateapplocals\guild.txt"));
        public static SocketGuild testGuild; //TODO: Only for testing
        private IServiceProvider _serviceProvider;

        private DiscordSocketClient _client;
        private readonly ConsoleLogger _logger = new("Program");

        static Task Main() => new Program().MainAsync(); //start

        public async Task MainAsync()
        {
            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All
            };

            _client = new DiscordSocketClient(config);
            _client.Log += Log;
            _client.Ready += Client_Ready;
            _client.SlashCommandExecuted += SlashCommandHandler;

            var token = File.ReadAllText(@"E:\archives\privateapplocals\token.txt");
            await _client.LoginAsync(TokenType.Bot, token);

            ////////////

            var serviceCollection = new ServiceCollection();
            SetServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();

            ////////////

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private void SetServices(IServiceCollection services)
        {
            services
                .AddScoped<BackupCommand, BackupCommand>()
                .AddScoped<BackupNotification, BackupNotification>()
                .AddScoped<BackupService, BackupService>()
                .AddScoped<AuthorRepository, AuthorRepository>()
                .AddScoped<BackupRegisterRepository, BackupRegisterRepository>()
                .AddScoped<ChannelRepository, ChannelRepository>()
                .AddScoped<MessageRepository, MessageRepository>();
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            _logger.BotActions($"{command.User.Username}: {command.CommandName}");

            switch (command.Data.Name)
            {
                case "backup":
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var bCommand = scope.ServiceProvider.GetService<BackupCommand>() ?? throw new ArgumentNullException();
                        await bCommand.BackupOptions(command);
                    }

                    var bc = new BackupCommand(new BackupService(new AuthorRepository(), new MessageRepository(), new BackupRegisterRepository(), new ChannelRepository()), new BackupNotification());
                    break;
            }
        }

        private Task Log(LogMessage msg) //TODO Assign slash commands / Make it to not over declare already existing commands
        {
            ConsoleLogger.BotLogger(msg.ToString());
            return Task.CompletedTask;
        }

        public async Task Client_Ready()
        {
            testGuild = _client.GetGuild(testGuildId);

            var guildCommand = new SlashCommandBuilder()
                   .WithName("backup")
                   .WithDescription("Backup de mensagens")
                   .AddOption
                   (
                        new SlashCommandOptionBuilder()
                       .WithName("fazer")
                       .WithDescription("Efetua o backup do canal")
                       .WithType(ApplicationCommandOptionType.SubCommandGroup)
                       .AddOption
                       (
                            new SlashCommandOptionBuilder()
                            .WithName("tudo")
                            .WithDescription("Efetua backup total do canal")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                       )
                       .AddOption
                       (
                            new SlashCommandOptionBuilder()
                            .WithName("ate-ultimo")
                            .WithDescription("Efetua backup até o ultimo backup realizado")
                            .WithType(ApplicationCommandOptionType.SubCommand)
                        )
                   )
                   .AddOption
                   (
                        new SlashCommandOptionBuilder()
                        .WithName("deletar")
                        .WithDescription("Deleta entradas no backup")
                        .WithType(ApplicationCommandOptionType.SubCommandGroup)
                        .AddOption
                        (
                            new SlashCommandOptionBuilder()
                            .WithName("proprio")
                            .WithDescription("Deletar as proprias mensagens do backup") //TODO Make a more meaningful warning message
                            .WithType(ApplicationCommandOptionType.SubCommand)
                        )
                   );



            try
            {
                await testGuild.CreateApplicationCommandAsync(guildCommand.Build());
            }
            catch (HttpException ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }
}