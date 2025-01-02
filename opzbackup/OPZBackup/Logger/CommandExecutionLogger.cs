using System.Text;
using Discord;
using Discord.WebSocket;
using Serilog;

namespace OPZBackup.Logger;

public class CommandExecutionLogger 
{

    private readonly ILogger _logger;

    public CommandExecutionLogger(ILogger logger)
    {
        _logger = logger.ForContext("System", "CommandExecution");
    }

    public void LogExecution(SocketInteraction interaction, SocketUser user)
    {
        var interactionData = ((ISlashCommandInteraction)interaction).Data;
        var command = GetCommandString(interactionData);

        _logger.Information("[{username}:{userId}] -> [{command}]",
            user.Username, user.Id, command);
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