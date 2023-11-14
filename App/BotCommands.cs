using Discord;
using Discord.Net;
using Discord.WebSocket;

namespace Bot
{
    internal class BotCommands
    {
        private readonly SocketGuild _guild;
        public BotCommands(SocketGuild guild)
        {
            _guild = guild;
        }


        public async Task Client_Ready()
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
                await _guild.CreateApplicationCommandAsync(guildCommand.Build());
            }
            catch (HttpException ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
    }
}
