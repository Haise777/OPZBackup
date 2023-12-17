using Discord;
using Discord.Interactions;

namespace OPZBot.Services.MessageBackup;

public class ResponseHandler
{
    private readonly ResponseBuilder _responseBuilder;
    private IMessage? _lastMessage;

    public ResponseHandler(ResponseBuilder responseBuilder)
    {
        _responseBuilder = responseBuilder;
    }

    public async Task SendStartNotificationAsync(object? sender, BackupEventArgs e)
    {
        var backupService = sender as BackupMessageService;
        _responseBuilder.Author = e.InteractionContext.User;
        _responseBuilder.StartTime = DateTime.Now;
        await e.InteractionContext.Interaction.RespondAsync(embed: _responseBuilder.Build(
            backupService.BatchNumber, backupService.SavedMessagesCount, backupService.SavedFilesCount,
            BackupStage.Started));
    }

    public async Task SendBatchFinishedAsync(object? sender, BackupEventArgs e)
    {
        var backupService = sender as BackupMessageService;
        _responseBuilder.StartMessage ??=
            await e.InteractionContext.Channel.GetMessageAsync(e.MessageBatch.Messages.First().Id);
        var currentMessage = await e.InteractionContext.Channel.GetMessageAsync(e.MessageBatch.Messages.Last().Id);
        _responseBuilder.CurrentMessage = currentMessage;


        await e.InteractionContext.Interaction.ModifyOriginalResponseAsync(m =>
            m.Embed = _responseBuilder.Build(
                backupService.BatchNumber, backupService.SavedMessagesCount, backupService.SavedFilesCount,
                BackupStage.InProgress));
        _lastMessage = currentMessage;
    }

    public async Task SendCompletedAsync(object? sender, BackupEventArgs e)
    {
        var backupService = sender as BackupMessageService;
        _responseBuilder.EndTime = DateTime.Now;
        _responseBuilder.LastMessage = _lastMessage;

        await e.InteractionContext.Interaction.ModifyOriginalResponseAsync(m =>
            m.Embed = _responseBuilder.Build(
                backupService.BatchNumber, backupService.SavedMessagesCount, backupService.SavedFilesCount,
                BackupStage.Finished));
        await GhostPing(e.InteractionContext);
    }

    public async Task SendFailedAsync(object? sender, BackupEventArgs e)
    {
        var backupService = sender as BackupMessageService;

        await e.InteractionContext.Interaction.ModifyOriginalResponseAsync(m =>
            m.Embed = _responseBuilder.Build(
                backupService.BatchNumber, backupService.SavedMessagesCount, backupService.SavedFilesCount,
                BackupStage.Failed));
        await GhostPing(e.InteractionContext);
    }

    private async Task GhostPing(SocketInteractionContext context)
    {
        var ping = await context.Interaction.FollowupAsync($"<@{context.User.Id}>");
        await Task.Delay(2000);
        await ping.DeleteAsync();
    }
}