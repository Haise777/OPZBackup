using Discord;
using Discord.Interactions;
using Discord.Rest;
using OPZBackup.Data.Models;
using OPZBackup.Services.Backup;

namespace OPZBackup.ResponseHandlers.Backup;

public class ServiceResponseHandler
{
    private readonly SocketInteractionContext _interactionContext;
    private readonly EmbedResponseFactory _embedResponseFactory;
    private RestFollowupMessage? _interaction;

    public ServiceResponseHandler(SocketInteractionContext interactionContext,
        EmbedResponseFactory embedResponseFactory)
    {
        _interactionContext = interactionContext;
        _embedResponseFactory = embedResponseFactory;
    }

    public async Task SendStartNotificationAsync(BackupContext context)
    {
        var embedResponse = _embedResponseFactory.StartMessageEmbed(context);
        _interaction = await _interactionContext.Interaction.FollowupAsync(embed: embedResponse);
    }

    public async Task SendBatchFinishedAsync(BackupContext context, IMessage startMessage, IMessage currentMessage)
    {
        if (_interaction == null) throw new InvalidOperationException("The interaction has not been created yet.");

        var embedResponse = _embedResponseFactory.BatchFinishedEmbed(context, startMessage, currentMessage);
        await _interaction.ModifyAsync(m => m.Embed = embedResponse);
    }

    public async Task SendCompletedAsync(BackupContext context, Channel channel, IMessage startMessage,
        IMessage lastMessage)
    {
        if (_interaction == null) throw new InvalidOperationException("The interaction has not been created yet.");

        var embedResponse = _embedResponseFactory.CompletedEmbed(context, startMessage, lastMessage, channel);
        await _interaction.ModifyAsync(m => m.Embed = embedResponse);
        await GhostPing();
    }

    public async Task SendFailedAsync(BackupContext context, Exception e, IMessage? startMessage)
    {
        if (_interaction == null) throw new InvalidOperationException("The interaction has not been created yet.");

        var embedResponse = _embedResponseFactory.FailedEmbed(context, e, startMessage);
        await _interaction.ModifyAsync(m => m.Embed = embedResponse);
        await GhostPing();
    }

    public async Task SendCompressingFilesAsync(BackupContext context, IMessage startMessage, IMessage currentMessage)
    {
        var embedResponse = _embedResponseFactory.CompressingEmbed(context, startMessage, currentMessage);
        await _interaction!.ModifyAsync(m => m.Embed = embedResponse);
    }

    public async Task SendProcessCancelledAsync()
    {
        if (_interaction == null) throw new InvalidOperationException("The interaction has not been created yet.");

        await _interaction.ModifyAsync(m =>
        {
            m.Content = "*O processo de backup foi cancelado*";
            m.Embed = null;
        });
    }

    private async Task GhostPing()
    {
        var ping = await _interactionContext.Channel.SendMessageAsync($"<@{_interactionContext.User.Id}>");
        await Task.Delay(2000);
        await ping.DeleteAsync();
    }
}