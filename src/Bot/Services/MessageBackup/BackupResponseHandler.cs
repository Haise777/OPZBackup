using Discord;
using Discord.Interactions;
using OPZBot.DataAccess.Models;

namespace OPZBot.Services.MessageBackup;

public class BackupResponseHandler
{
    private readonly BackupResponseBuilder _responseBuilder;
    private IMessage? _lastMessage; //TODO Would that also be a SRP violation?
    private int _numberOfMessages;
    private int _batchNumber;

    public event Func<Exception, Task>? NotifyException;

    public BackupResponseHandler(BackupResponseBuilder responseBuilder)
    {
        _responseBuilder = responseBuilder;
    }

    public async Task SendStartNotification(SocketInteractionContext context, BackupRegistry registry)
    {
        await RunUnderErrorNotifier(async () =>
        {
            _responseBuilder.Author = context.User;
            _responseBuilder.StartTime = DateTime.Now;
            await context.Interaction.RespondAsync(embed: _responseBuilder.Build(
                _batchNumber, _numberOfMessages, BackupStage.Started));
        });
    }

    public async Task SendBatchFinished(SocketInteractionContext context, MessageDataBatchDto messageBatchDto)
    {
        await RunUnderErrorNotifier(async () =>
        {
            _responseBuilder.StartMessage ??= messageBatchDto.Messages.First();
            _responseBuilder.LastMessage = messageBatchDto.Messages.Last();
            _numberOfMessages += messageBatchDto.Messages.Count();
            _batchNumber++;

            await context.Interaction.ModifyOriginalResponseAsync(m =>
                m.Embed = _responseBuilder.Build(
                    _batchNumber, _numberOfMessages, BackupStage.InProgress));
            _lastMessage = messageBatchDto.Messages.Last();
        });
    }

    public async Task SendBackupCompleted(SocketInteractionContext context)
    {
        await RunUnderErrorNotifier(async () =>
        {
            _responseBuilder.EndTime = DateTime.Now;
            _responseBuilder.LastMessage = _lastMessage;

            await context.Interaction.ModifyOriginalResponseAsync(m =>
                m.Embed = _responseBuilder.Build(
                    _batchNumber, _numberOfMessages, BackupStage.Finished));

            await GhostPing(context);
        });
    }

    public async Task SendBackupFailed(SocketInteractionContext context, Exception ex)
    {
        await RunUnderErrorNotifier(async () =>
        {
            await context.Interaction.ModifyOriginalResponseAsync(m =>
                m.Embed = _responseBuilder.Build(
                    _batchNumber, _numberOfMessages, BackupStage.Failed));

            await GhostPing(context);
        });
    }

    private async Task GhostPing(SocketInteractionContext context)
    {
        var ping = await context.Interaction.FollowupAsync($"@<{context.User.Id}>");
        await Task.Delay(2000);
        await ping.DeleteAsync();
    }

    private async Task RunUnderErrorNotifier(Func<Task> run) //TODO Actually implement a error handler for this scenario
    {
        try
        {
            await run();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await NotifyException?.Invoke(e);
        }
    }
}