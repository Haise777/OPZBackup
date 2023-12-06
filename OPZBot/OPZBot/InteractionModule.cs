using Discord.Interactions;

namespace OPZBot;

public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Receive a ping")]
    public async Task HandlePingCommand()
    {
        await RespondAsync("PING");
    }

}