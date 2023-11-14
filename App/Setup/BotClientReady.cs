using Discord;
using Discord.Net;
using Discord.WebSocket;

namespace Bot.Setup
{
    internal class BotClientReady
    {
        private readonly DiscordSocketClient _client;
        private readonly ulong _guildId;
        public BotClientReady(DiscordSocketClient socketClient, ulong guildId)
        {
            _client = socketClient;
            _guildId = guildId;
        }

        public async Task Client_Ready()
        {
            var socketGuild = _client.GetGuild(_guildId);
            Program.Guild = socketGuild;

            await RegisterCommands(socketGuild);
        }
        private async Task RegisterCommands(SocketGuild guild)
        {
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
                await guild.CreateApplicationCommandAsync(guildCommand.Build());
            }
            catch (HttpException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
