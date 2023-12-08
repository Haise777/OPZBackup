using Discord.Interactions;

namespace OPZBot;

[Group("backup","utilizar a função de backup")]
public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("fazer", "efetua backup deste canal")]
    public async Task HandlePingCommand([Choice("ate-ultimo", 0), Choice("total", 1)] string oi)
    {
        await RespondAsync("PING");
    }
    
    

}