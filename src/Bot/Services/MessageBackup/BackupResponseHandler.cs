using Discord;
using Discord.Interactions;
using OPZBot.DataAccess.Models;

namespace OPZBot.Services.MessageBackup;

public class BackupResponseHandler
{
    private readonly BackupResponseBuilder _responseBuilder;
    private IMessage? _lastMessage;
    
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
            await context.Interaction.RespondAsync(embed: _responseBuilder.Build(BackupStage.Started));
        });
    }

    public async Task SendBatchFinished(SocketInteractionContext context, MessageDataBatch messageBatch)
    {
        await RunUnderErrorNotifier(async () =>
        {
            _responseBuilder.StartMessage ??= messageBatch.Messages.First();
            _responseBuilder.LastMessage = messageBatch.Messages.Last();

            await context.Interaction.ModifyOriginalResponseAsync(m =>
                m.Embed = _responseBuilder.Build(BackupStage.InProgress));
            _lastMessage = messageBatch.Messages.Last();
        });
    }

    public async Task SendBackupCompleted(SocketInteractionContext context)
    {
        await RunUnderErrorNotifier(async () =>
        {
            _responseBuilder.EndTime = DateTime.Now;
            _responseBuilder.LastMessage = _lastMessage;

            await context.Interaction.ModifyOriginalResponseAsync(m =>
                m.Embed = _responseBuilder.Build(BackupStage.Finished));
        });
    }

    public async Task SendBackupFailed(SocketInteractionContext context, Exception ex)
    {
        await RunUnderErrorNotifier(async () =>
        {
            Console.WriteLine(ex);
            throw new NotImplementedException();
        });
    }
    
    
    private async Task RunUnderErrorNotifier(Func<Task> run) //TODO Test if it actually works
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