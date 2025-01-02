using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;
using OPZBackup.Logger;
using Serilog;

namespace OPZBackup;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;
    private readonly CommandExecutionLogger _logger;

    public InteractionHandler(DiscordSocketClient client,
        InteractionService commands,
        IServiceProvider services,
        CommandExecutionLogger logger)
    {
        _client = client;
        _commands = commands;
        _services = services;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        _client.InteractionCreated += HandleInteraction;
    }

    //Redirect the client's 'interaction' to its appropriate handler
    private async Task HandleInteraction(SocketInteraction arg)
    {
        //TODO: Implement a command cooldown for the same user
        try
        {
            var ctx = new SocketInteractionContext(_client, arg);

            _logger.LogExecution(arg, ctx.User);
            await _commands.ExecuteCommandAsync(ctx, _services);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error on redirecting interaction to it's module");
            throw;
        }
    }
}