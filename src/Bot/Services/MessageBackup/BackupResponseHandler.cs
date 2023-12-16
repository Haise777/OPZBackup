using Discord;
using Discord.Interactions;
using OPZBot.DataAccess.Models;

namespace OPZBot.Services.MessageBackup;

public class BackupResponseHandler
{
    private readonly BackupResponseBuilder _responseBuilder;
    private IMessage? _lastMessage;
    private int _batchNumber;
    private int _numberOfMessages;

    public BackupResponseHandler(BackupResponseBuilder responseBuilder)
    {
        _responseBuilder = responseBuilder;
    }

    public async Task SendStartNotification(SocketInteractionContext context, BackupRegistry registry)
    {
        _responseBuilder.Author = context.User;
        _responseBuilder.StartTime = DateTime.Now;
        await context.Interaction.RespondAsync(embed: _responseBuilder.Build(
            _batchNumber, _numberOfMessages, BackupStage.Started));
    }

    public async Task SendBatchFinished(SocketInteractionContext context, MessageDataBatchDto messageBatchDto, BackupRegistry registry)
    {
        _responseBuilder.StartMessage ??= messageBatchDto.Messages.First();
        _responseBuilder.LastMessage = messageBatchDto.Messages.Last();
        _numberOfMessages += messageBatchDto.Messages.Count();
        _batchNumber++;

        await context.Interaction.ModifyOriginalResponseAsync(m =>
            m.Embed = _responseBuilder.Build(
                _batchNumber, _numberOfMessages, BackupStage.InProgress));
        _lastMessage = messageBatchDto.Messages.Last();
    }

    public async Task SendBackupCompleted(SocketInteractionContext context, BackupRegistry registry)
    {
        _responseBuilder.EndTime = DateTime.Now;
        _responseBuilder.LastMessage = _lastMessage;

        await context.Interaction.ModifyOriginalResponseAsync(m =>
            m.Embed = _responseBuilder.Build(
                _batchNumber, _numberOfMessages, BackupStage.Finished));

        await GhostPing(context);
    }

    public async Task SendBackupFailed(SocketInteractionContext context, Exception ex, BackupRegistry registry)
    {
        await context.Interaction.ModifyOriginalResponseAsync(m =>
            m.Embed = _responseBuilder.Build(
                _batchNumber, _numberOfMessages, BackupStage.Failed));

        await GhostPing(context);
    }

    private async Task GhostPing(SocketInteractionContext context)
    {
        var ping = await context.Interaction.FollowupAsync($"<@{context.User.Id}>");
        await Task.Delay(2000);
        await ping.DeleteAsync();
    }
}