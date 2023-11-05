using App.Modules;
using Discord;
using Discord.Net;
using Discord.WebSocket;

namespace App
{
    internal class Program
    {
        BackupChannel _backupChannel = new BackupChannel();

        public static ulong testGuild = ulong.Parse(File.ReadAllText(@"E:\archives\token\guild.txt"));
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



            var token = File.ReadAllText(@"E:\archives\token\token.txt");
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
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public async Task Client_Ready()
        {
            var guild = _client.GetGuild(testGuild);


            var guildCommand = new SlashCommandBuilder()
                   .WithName("backup")
                   .WithDescription("gerenciar backups de mensagens")
                   .AddOption(new SlashCommandOptionBuilder()
                       .WithName("fazer")
                       .WithDescription("Gets or sets the field A")
                       .WithType(ApplicationCommandOptionType.SubCommandGroup)
                       .AddOption(new SlashCommandOptionBuilder()
                           .WithName("valor")
                           .WithDescription("Sets the field A")
                           .WithType(ApplicationCommandOptionType.SubCommand)
                           .AddOption("value", ApplicationCommandOptionType.String, "the value to set the field", isRequired: true))
                       .AddOption(new SlashCommandOptionBuilder()
                           .WithName("tudo")
                           .WithDescription("Gets the value of field A.")
                           .WithType(ApplicationCommandOptionType.SubCommand))
                       .AddOption(new SlashCommandOptionBuilder()
                           .WithName("ultimo-backup")
                           .WithDescription("balblalba")
                           .WithType(ApplicationCommandOptionType.SubCommand))
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