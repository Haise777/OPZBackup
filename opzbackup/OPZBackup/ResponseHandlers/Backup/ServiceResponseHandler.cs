using Discord.Interactions;
using Discord.Rest;
using OPZBackup.Data.Models;
using OPZBackup.Services;
using OPZBackup.Services.Backup;

namespace OPZBackup.ResponseHandlers.Backup;

public class ServiceResponseHandler
{
    private readonly SocketInteractionContext _interactionContext;
    private readonly ResponseBuilder _responseBuilder;
    private RestFollowupMessage? _interaction;

    public ServiceResponseHandler(SocketInteractionContext interactionContext,
        ResponseBuilder responseBuilder)
    {
        _interactionContext = interactionContext;
        _responseBuilder = responseBuilder;
    }

    public async Task SendStartNotificationAsync(BackupContext context)
    {
        var embedResponse = _responseBuilder
            .SetStartTime(DateTime.Now)
            .SetBatchNumber(context.BatchNumber)
            .SetMessageCount(context.MessageCount)
            .SetFileCount(context.FileCount)
            .Build(ProgressStage.Started);

        _interaction = await _interactionContext.Interaction.FollowupAsync(embed: embedResponse);
    }

    public async Task SendBatchFinishedAsync(BackupContext context, BackupBatch batch, TimeSpan averageBatchTime)
    {
        if (_interaction == null) throw new InvalidOperationException("The interaction has not been created yet.");

        if (_responseBuilder.StartMessage == null)
            _responseBuilder.SetStartMessage(
                await _interactionContext.Channel.GetMessageAsync(batch.ProcessedMessages.First().Id));

        var currentMessage = await _interactionContext.Channel.GetMessageAsync(batch.ProcessedMessages.Last().Id);

        var embedResponse = _responseBuilder
            .SetCurrentMessage(currentMessage)
            .SetBatchNumber(context.BatchNumber)
            .SetMessageCount(context.MessageCount)
            .SetFileCount(context.FileCount)
            .SetTotalFileSize(context.StatisticTracker.GetTotalStatistics().ByteSize)
            .SetAverageBatchTime(averageBatchTime)
            .UpdateElapsedTime()
            .Build(ProgressStage.InProgress);

        await _interaction.ModifyAsync(m => m.Embed = embedResponse);
    }

    public async Task SendCompletedAsync(BackupContext context, Channel channel)
    {
        if (_interaction == null) throw new InvalidOperationException("The interaction has not been created yet.");

        var embedResponse = _responseBuilder
            .CurrentAsLastMessage()
            .SetBatchNumber(context.BatchNumber)
            .SetMessageCount(context.MessageCount)
            .SetFileCount(context.FileCount)
            .UpdateElapsedTime()
            .SetChannelMessages(channel.MessageCount)
            .SetChannelByteSize(channel.ByteSize)
            .SetChannelNumberOfFiles(channel.FileCount)
            .SetChannelCompressedSize(channel.CompressedByteSize)
            .Build(ProgressStage.Finished);

        await _interaction.ModifyAsync(m => m.Embed = embedResponse);
        await GhostPing();
    }

    public async Task SendFailedAsync(BackupContext context)
    {
        if (_interaction == null) throw new InvalidOperationException("The interaction has not been created yet.");

        var embedResponse = _responseBuilder
            .SetBatchNumber(context.BatchNumber)
            .SetMessageCount(context.MessageCount)
            .SetFileCount(context.FileCount)
            .UpdateElapsedTime()
            .Build(ProgressStage.Failed);

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