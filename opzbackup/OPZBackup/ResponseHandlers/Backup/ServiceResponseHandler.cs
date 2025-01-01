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
    private IMessage? _startMessage;
    private IMessage? _lastMessage;
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

    public async Task SendBatchFinishedAsync(BackupContext context, BackupBatch batch, TimeSpan averageBatchTime)
    {
        if (_interaction == null) throw new InvalidOperationException("The interaction has not been created yet.");

        if (_startMessage is null)
        {
            var startMessage = await _interactionContext.Channel.GetMessageAsync(batch.ProcessedMessages.First().Id);
            _startMessage = startMessage;
        }

        var currentMessage = await _interactionContext.Channel.GetMessageAsync(batch.ProcessedMessages.Last().Id);
        _lastMessage = currentMessage;

        var embedResponse = _embedResponseFactory.BatchFinishedEmbed(context, _startMessage, currentMessage);
        await _interaction.ModifyAsync(m => m.Embed = embedResponse);
    }

    public async Task SendCompletedAsync(BackupContext context, Channel channel)
    {
        if (_interaction == null) throw new InvalidOperationException("The interaction has not been created yet.");

        var embedResponse = _embedResponseFactory.CompletedEmbed(context, _startMessage, _lastMessage, channel);
        await _interaction.ModifyAsync(m => m.Embed = embedResponse);
        await GhostPing();
    }

    public async Task SendFailedAsync(BackupContext context, Exception e)
    {
        if (_interaction == null) throw new InvalidOperationException("The interaction has not been created yet.");

        var embedResponse = _embedResponseFactory.FailedEmbed(context, e);
        await _interaction.ModifyAsync(m => m.Embed = embedResponse);
        await GhostPing();
    }

    public async Task SendCompressingFilesAsync(BackupContext context)
    {
        //TODO-2 Work in a better message for this
        await _interaction!.ModifyAsync(m =>
        {
            m.Content = "*Arquivos estao sendo comprimidos agora*";
            m.Embed = null;
        });
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