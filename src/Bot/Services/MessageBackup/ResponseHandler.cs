// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord;
using Discord.Interactions;
using OPZBot.Modules;

namespace OPZBot.Services.MessageBackup;

public class ResponseHandler(ResponseBuilder responseBuilder) : IResponseHandler
{
    private ulong _interactionMessageId;
    private IMessage? _lastMessage;

    public async Task SendStartNotificationAsync(object? sender, BackupEventArgs e)
    {
        var backupService = sender as BackupMessageService;
        responseBuilder.Author = e.InteractionContext.User;
        responseBuilder.StartTime = DateTime.Now;
        _interactionMessageId = (await e.InteractionContext.Interaction.FollowupAsync(
            embed: responseBuilder.Build(
                backupService.BatchNumber, backupService.SavedMessagesCount, backupService.SavedFilesCount,
                ProgressStage.Started))).Id;
    }

    public async Task SendBatchFinishedAsync(object? sender, BackupEventArgs e)
    {
        var backupService = sender as BackupMessageService;
        responseBuilder.StartMessage ??=
            await e.InteractionContext.Channel.GetMessageAsync(e.MessageBatch.Messages.First().Id);
        var currentMessage = await e.InteractionContext.Channel.GetMessageAsync(e.MessageBatch.Messages.Last().Id);
        responseBuilder.CurrentMessage = currentMessage;

        await e.InteractionContext.Channel.ModifyMessageAsync(_interactionMessageId, m =>
            m.Embed = responseBuilder.Build(
                backupService.BatchNumber, backupService.SavedMessagesCount, backupService.SavedFilesCount,
                ProgressStage.InProgress));
        _lastMessage = currentMessage;
    }

    public async Task SendCompletedAsync(object? sender, BackupEventArgs e)
    {
        var backupService = sender as BackupMessageService;
        responseBuilder.EndTime = DateTime.Now;
        responseBuilder.LastMessage = _lastMessage;

        await e.InteractionContext.Channel.ModifyMessageAsync(_interactionMessageId, m =>
            m.Embed = responseBuilder.Build(
                backupService.BatchNumber, backupService.SavedMessagesCount, backupService.SavedFilesCount,
                ProgressStage.Finished));
        await GhostPing(e.InteractionContext);
    }

    public async Task SendFailedAsync(object? sender, BackupEventArgs e)
    {
        var backupService = sender as BackupMessageService;

        await e.InteractionContext.Channel.ModifyMessageAsync(_interactionMessageId, m =>
            m.Embed = responseBuilder.Build(
                backupService.BatchNumber, backupService.SavedMessagesCount, backupService.SavedFilesCount,
                ProgressStage.Failed));
        await GhostPing(e.InteractionContext);
    }

    public async Task SendInvalidAttemptAsync(SocketInteractionContext context, TimeSpan cooldownTime)
    {
        var formattedTime = cooldownTime > TimeSpan.FromHours(0.99)
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

    public async Task SendAlreadyInProgressAsync(SocketInteractionContext context)
    {
        await context.Interaction.RespondAsync(
            "*Por limitações do Discord, não é possivel efetuar mais de um processo de backup simutaneamente*");

        await Task.Delay(7000);
        await context.Interaction.DeleteOriginalResponseAsync();
    }

    public async Task SendNotRightPermissionAsync(SocketInteractionContext context)
    {
        await context.Interaction.RespondAsync("*Você não possui as permissões adequadas para este comando*");
        await Task.Delay(7000);
        await context.Interaction.DeleteOriginalResponseAsync();
    }

    private async Task GhostPing(SocketInteractionContext context)
    {
        var ping = await context.Channel.SendMessageAsync($"<@{context.User.Id}>");
        await Task.Delay(2000);
        await ping.DeleteAsync();
    }
}