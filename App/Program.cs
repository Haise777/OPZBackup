using App.Modules;
using Discord;
using Discord.Net;
using Discord.WebSocket;

namespace App
{
    internal class Program
    {
        BackupChannel _backupChannel = new BackupChannel();

        public static ulong testGuild = ulong.Parse(File.ReadAllText(@"E:\archives\privateapplocals\guild.txt"));
        private DiscordSocketClient _client;
        static Task Main(string[] args) => new Program().MainAsync();

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

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {

            switch (command.Data.Name)
            {
                case "backup":
                    await _backupChannel.BackupOptions(command);
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
            var guild = _client.GetGuild(testGuild);


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
                           .WithName("total")
                           .WithDescription("Faz o backup total do canal")
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
                            .AddOption("confirmar", ApplicationCommandOptionType.Boolean, "confirmar", isRequired: true)
                        )
                   );



            try
            {
                await guild.CreateApplicationCommandAsync(guildCommand.Build());
            }
            catch (HttpException ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }
}