using Discord.Interactions;
using Discord.WebSocket;
using OPZBackup.Modules;

namespace OPZBackup;

public class StatsInteractionHandler
{
    private readonly StatInteractionCache _interactionCache;
    private readonly DiscordSocketClient _client;

    public StatsInteractionHandler(StatInteractionCache interactionCache, DiscordSocketClient client)
    {
        _interactionCache = interactionCache;
        _client = client;
    }

    public async Task HandleInteraction(SocketMessageComponent component)
    {
        var componentIdElements = component.Data.CustomId.Split('-');
        var ctx = new SocketInteractionContext(_client, component);

        switch (componentIdElements[0])
        {
            case "detailedchannel": await detailedChannelExecuted(ctx, componentIdElements); break;
            case "users": await usersExecuted(ctx, componentIdElements); break;
            case "channels": await channelsExecuted(ctx, componentIdElements); break;
            case "detaileduser": await detailedUserExecuted(component ,ctx, componentIdElements); break;
        }
    }

    private async Task detailedUserExecuted(SocketMessageComponent component, SocketInteractionContext ctx, string[] componentIdElements)
    {
        var interactionState = _interactionCache.GetDetailedUserInteraction(ulong.Parse(componentIdElements[2]));
        if (interactionState == null)
            return;
        
        switch (componentIdElements[1])
        {
            case "advancepage":
                await interactionState.AdvancePage(ctx);
                break;
            case "returnpage":
                await interactionState.ReturnPage(ctx);
                break;
            case "tableswitch":
                await interactionState.SwitchTable(ctx, component.Data.Values.First());
                break;
        }
    }

    private async Task channelsExecuted(SocketInteractionContext ctx, string[] componentIdElements)
    {
        var interactionState = _interactionCache.GetChannelsInteraction(ulong.Parse(componentIdElements[2]));
        if (interactionState == null)
            return;
        
        switch (componentIdElements[1])
        {
            case "advancepage":
                await interactionState.AdvancePage(ctx);
                break;
            case "returnpage":
                await interactionState.ReturnPage(ctx);
                break;
        }
    }

    private async Task usersExecuted(SocketInteractionContext ctx, string[] componentIdElements)
    {
        var interactionState = _interactionCache.GetUsersInteraction(ulong.Parse(componentIdElements[2]));
        if (interactionState == null)
            return;
        
        switch (componentIdElements[1])
        {
            case "advancepage":
                await interactionState.AdvancePage(ctx);
                break;
            case "returnpage":
                await interactionState.ReturnPage(ctx);
                break;
        }
    }

    private async Task detailedChannelExecuted(SocketInteractionContext interactionContext, string[] componentIdElements)
    {
        var interactionState = _interactionCache.GetDetailedChannelInteraction(ulong.Parse(componentIdElements[2]));
        if (interactionState == null)
            return;
        
        switch (componentIdElements[1])
        {
            case "advancepage":
                await interactionState.AdvancePage(interactionContext);
                break;
            case "returnpage":
                await interactionState.ReturnPage(interactionContext);
                break;
        }
    }
}