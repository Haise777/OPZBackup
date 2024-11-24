using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog;

namespace OPZBackup;

public class InteractionHandler(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
{
    public async Task InitializeAsync()
    {
        await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        client.InteractionCreated += HandleInteraction;
    }

    //Redirect the client's 'interaction' to its appropriate handler
    private async Task HandleInteraction(SocketInteraction arg)
    {
        //TODO-3 Implement a command cooldown for the same user
        try
        {
            var ctx = new SocketInteractionContext(client, arg);
            await commands.ExecuteCommandAsync(ctx, services);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error on redirecting interaction to it's module");
            throw;
        }
    }
}