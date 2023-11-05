using Discord;
using Discord.Net;
using Discord.WebSocket;

namespace App
{
    internal class Program
    {
        public static ulong testGuild = ulong.Parse(File.ReadAllText(@"E:\archives\token\guild.txt"));
        private DiscordSocketClient _client;
        static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
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
            await command.RespondAsync($"You executed {command.Data}");
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public async Task Client_Ready()
        {
            var guild = _client.GetGuild(testGuild);

            var guildCommand = new SlashCommandBuilder();

            guildCommand.WithName("ping");
            guildCommand.WithDescription("gets the bot's response latency");

            try
            {
                await guild.CreateApplicationCommandAsync(guildCommand.Build());
            }
            catch (HttpException ex)
            {
                Console.WriteLine(ex.Errors);
            }

        }
    }
}