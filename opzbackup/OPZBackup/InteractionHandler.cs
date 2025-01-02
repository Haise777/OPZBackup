using System.Reflection;
using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Serilog;

namespace OPZBackup;

public class InteractionHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;
    private readonly ILogger _logger;

    public InteractionHandler(DiscordSocketClient client,
        InteractionService commands,
        IServiceProvider services,
        ILogger logger)
    {
        _client = client;
        _commands = commands;
        _services = services;
        _logger = logger.ForContext("System", "InteractionHandler");
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

            var interactionData = ((ISlashCommandInteraction)arg).Data;
            var command = GetCommandString(interactionData);

            _logger.Information("[{username}:{userId}] -> [{command}]",
                ctx.User.Username, ctx.User.Id, command);

            await _commands.ExecuteCommandAsync(ctx, _services);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error on redirecting interaction to it's module");
            throw;
        }
    }

    private static string GetCommandString(IApplicationCommandInteractionData interaction)
    {
        var builder = new StringBuilder();
        builder.Append(interaction.Name + " ");

        var options = interaction.Options;
        RecursiveStringBuild(builder, options);

        return builder.ToString();
    }

    private static StringBuilder RecursiveStringBuild(StringBuilder builder,
        IReadOnlyCollection<IApplicationCommandInteractionDataOption> options)
    {
        foreach (var option in options)
        {
            builder.Append(" > " + $"{option.Name}");
            
            if (option.Value != null)
                builder.Append($":{option.Value}");
            
            if (option.Options.Any())
                RecursiveStringBuild(builder, option.Options);
        }

        return builder;
    }
}