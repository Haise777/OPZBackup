using Discord;
using Discord.Interactions;
using OPZBot.Modules;

namespace OPZBot.Services.MessageBackup;

public class ResponseHandler : IResponseHandler
{
    private readonly ResponseBuilder _responseBuilder;
    private ulong _interactionMessageId;
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
        _interactionMessageId = (await e.InteractionContext.Interaction.FollowupAsync(
            embed: _responseBuilder.Build(
                backupService.BatchNumber, backupService.SavedMessagesCount, backupService.SavedFilesCount,
                ProgressStage.Started))).Id;
    }

    public async Task SendBatchFinishedAsync(object? sender, BackupEventArgs e)
    {
        var backupService = sender as BackupMessageService;
        _responseBuilder.StartMessage ??=
            await e.InteractionContext.Channel.GetMessageAsync(e.MessageBatch.Messages.First().Id);
        var currentMessage = await e.InteractionContext.Channel.GetMessageAsync(e.MessageBatch.Messages.Last().Id);
        _responseBuilder.CurrentMessage = currentMessage;

        await e.InteractionContext.Channel.ModifyMessageAsync(_interactionMessageId, m =>
            m.Embed = _responseBuilder.Build(
                backupService.BatchNumber, backupService.SavedMessagesCount, backupService.SavedFilesCount,
                ProgressStage.InProgress));
        _lastMessage = currentMessage;
    }

    public async Task SendCompletedAsync(object? sender, BackupEventArgs e)
    {
        var backupService = sender as BackupMessageService;
        _responseBuilder.EndTime = DateTime.Now;
        _responseBuilder.LastMessage = _lastMessage;

        await e.InteractionContext.Channel.ModifyMessageAsync(_interactionMessageId, m =>
            m.Embed = _responseBuilder.Build(
                backupService.BatchNumber, backupService.SavedMessagesCount, backupService.SavedFilesCount,
                ProgressStage.Finished));
        await GhostPing(e.InteractionContext);
    }

    public async Task SendFailedAsync(object? sender, BackupEventArgs e)
    {
        var backupService = sender as BackupMessageService;

        await e.InteractionContext.Channel.ModifyMessageAsync(_interactionMessageId, m =>
            m.Embed = _responseBuilder.Build(
                backupService.BatchNumber, backupService.SavedMessagesCount, backupService.SavedFilesCount,
                ProgressStage.Failed));
        await GhostPing(e.InteractionContext);
    }

    private async Task GhostPing(SocketInteractionContext context)
    {
        var ping = await context.Channel.SendMessageAsync($"<@{context.User.Id}>");
        await Task.Delay(2000);
        await ping.DeleteAsync();
    }

    public async Task SendInvalidAttemptAsync(SocketInteractionContext context, TimeSpan cooldownTime)
    {
        var formattedTime = cooldownTime > TimeSpan.FromHours(1)
            ? $"{cooldownTime.Hours} horas e {cooldownTime.Minutes} minutos"
            : $"{cooldownTime.Minutes} minutos e {cooldownTime.Seconds} segundos";

        await context.Interaction.FollowupAsync("Tentativa de backup inválida" +
                                                $"\n**{formattedTime}** restantes");
        await Task.Delay(7000);
        await context.Interaction.DeleteOriginalResponseAsync();
    }

    public async Task SendDeleteConfirmationAsync(SocketInteractionContext context)
    {
        var button = new ButtonBuilder()
            .WithStyle(ButtonStyle.Danger)
            .WithLabel("Confirmar")
            .WithCustomId(BackupInteractionModule.CONFIRM_USER_DELETE_ID);
        var buttonCancel = new ButtonBuilder()
            .WithStyle(ButtonStyle.Secondary)
            .WithLabel("Cancelar")
            .WithCustomId(BackupInteractionModule.CANCEL_USER_DELETE_ID);

        var components = new ComponentBuilder()
            .WithButton(button)
            .WithButton(buttonCancel)
            .Build();

        await context.Interaction.RespondAsync(ephemeral: true, text:
            "**Todas as suas mensagens** junto de seu usuario serão apagados dos registros de backup permanentemente" +
            "\nDeseja prosseguir?", components: components);
    }

    public async Task SendUserDeletionResultAsync(SocketInteractionContext context, bool wasDeleted)
    {
        if (wasDeleted)
        {
            await context.Interaction.DeferAsync();
            await context.Interaction.DeleteOriginalResponseAsync();
            await context.Interaction.FollowupAsync(
                $"***{context.User.Username}** foi deletado dos registros de backup*");
            return;
        }

        await context.Interaction.DeferAsync();
        await context.Interaction.DeleteOriginalResponseAsync();
    }

    public async Task SendEmptyBackupAsync(object? sender, BackupEventArgs args)
    {
        await args.InteractionContext.Channel.ModifyMessageAsync(_interactionMessageId, m =>
        {
            m.Content = "*Tentativa de backup inválida: Não havia mensagens válidas para serem salvas*";
            m.Embed = null;
        });

        await Task.Delay(7000);
        await args.InteractionContext.Interaction.DeleteOriginalResponseAsync();
    }
}