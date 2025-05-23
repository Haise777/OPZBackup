using Discord.Interactions;
using Discord.WebSocket;
using OPZBackup.Services.Stats;

namespace OPZBackup.ResponseHandlers.Stats;

public class StatsInteractionHandler
{
    private readonly StatInteractionStateManager _interactionStateManager;
    private readonly ResponseHandler _responseHandler;
    private readonly DiscordSocketClient _client;

    public StatsInteractionHandler(StatInteractionStateManager interactionStateManager, DiscordSocketClient client, ResponseHandler responseHandler)
    {
        _interactionStateManager = interactionStateManager;
        _client = client;
        _responseHandler = responseHandler;
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
        var interactionState = _interactionStateManager.GetDetailedUserInteraction(ulong.Parse(componentIdElements[2]));
        if (interactionState == null)
            return;
        
        switch (componentIdElements[1])
        {
            case "advancepage":
                await AdvancePage(interactionState, ctx);
                break;
            case "returnpage":
                await ReturnPage(interactionState, ctx);
                break;
            case "tableswitch":
                await SwitchTable(interactionState,component.Data.Values.First() ,ctx);
                break;
        }

        await _responseHandler.SendUpdatedUserStatsAsync(interactionState, interactionState.Interaction);
    }
    
    private async Task channelsExecuted(SocketInteractionContext ctx, string[] componentIdElements)
    {
        var interactionState = _interactionStateManager.GetChannelsInteraction(ulong.Parse(componentIdElements[2]));
        if (interactionState == null)
            return;
        
        await ExecuteComponentAction(ctx, componentIdElements, interactionState);
        
        await _responseHandler.SendUpdateChannelsStatsAsync(interactionState, interactionState.interactionContext);
    }
    
    private async Task usersExecuted(SocketInteractionContext ctx, string[] componentIdElements)
    {
        var interactionState = _interactionStateManager.GetUsersInteraction(ulong.Parse(componentIdElements[2]));
        if (interactionState == null)
            return;
        
        await ExecuteComponentAction(ctx, componentIdElements, interactionState);
        await _responseHandler.SendUpdateUsersStatsAsync(interactionState, interactionState.interactionContext);
    }

    private async Task detailedChannelExecuted(SocketInteractionContext ctx, string[] componentIdElements)
    {
        var interactionState = _interactionStateManager.GetDetailedChannelInteraction(ulong.Parse(componentIdElements[2]));
        if (interactionState == null)
            return;
        
        await ExecuteComponentAction(ctx, componentIdElements, interactionState);
        await _responseHandler.SendUpdatedDetailedChannelStatsAsync(interactionState, interactionState.interactionContext);
    }
    
    private async Task ExecuteComponentAction<T>(SocketInteractionContext ctx, string[] componentIdElements,
        InteractionState<T> interactionState)
    {
        switch (componentIdElements[1])
        {
            case "advancepage":
                await AdvancePage(interactionState, ctx);
                break;
            case "returnpage":
                await ReturnPage(interactionState, ctx);
                break;
            default:
                throw new InvalidOperationException("Invalid action on ComponentID");
        }
    }

    private async Task AdvancePage<T>(InteractionState<T> state, SocketInteractionContext ctx)
    {
        // await ctx.Interaction.DeferAsync();
        state.CurrentPage++;
    }
    private async Task ReturnPage<T>(InteractionState<T> state, SocketInteractionContext ctx)
    {
        // await ctx.Interaction.DeferAsync();
        state.CurrentPage--;
    }
    private async Task AdvancePage<T>(MultiTableInteractionState<T> state, SocketInteractionContext ctx)
    {
        // await ctx.Interaction.DeferAsync();
        state.currentTablePage[state.selectBoxOption]++;
    }
    private async Task ReturnPage<T>(MultiTableInteractionState<T> state, SocketInteractionContext ctx)
    {
        // await ctx.Interaction.DeferAsync();
        state.currentTablePage[state.selectBoxOption]--;
    }
    private async Task SwitchTable<T>(MultiTableInteractionState<T> interactionState, string value, SocketInteractionContext ctx)
    {
        // await ctx.Interaction.DeferAsync();
        interactionState.selectBoxOption = value;
    }
}