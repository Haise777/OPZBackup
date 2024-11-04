// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using OPZBackup.Services;
using OPZBot.Logging;
using OPZBot.Modules;
using OPZBot.Services.MessageBackup;
using Serilog;

namespace OPZBackup.ResponseHandlers;

public class BackupResponseHandler
{
    private RestFollowupMessage? _interactionMessage;
    private IMessage? _lastMessage;
    private EmbedResponseBuilder? _responseBuilder;
    
    //TODO-3 Make it so that it stores the SocketInteractionContext from the BackupModule
    //TODO-3 Add a method that gives a object that separates all of the EmbedResponse methods from the rest

    public async Task SendStartNotificationAsync(SocketInteractionContext interactionContext, BackupContext context)
    {
        _responseBuilder = EmbedResponseBuilder.GetBuilder();

        var embedResponse = _responseBuilder
            .SetAuthor(interactionContext.User)
            .SetStartTime(DateTime.Now)
            .SetBatchNumber(context.BatchNumber)
            .SetMessageCount(context.MessageCount)
            .SetFileCount(context.FileCount)
            .Build(ProgressStage.Started);

        _interactionMessage = await interactionContext.Interaction.FollowupAsync(embed: embedResponse);
    }

    public async Task SendBatchFinishedAsync(SocketInteractionContext interactionContext, BackupContext context, BackupBatch batch)
    {
        if (_responseBuilder.StartMessage == null)
            _responseBuilder.SetStartMessage(
                await interactionContext.Channel.GetMessageAsync(batch.Messages.First().Id));

        var currentMessage = await interactionContext.Channel.GetMessageAsync(batch.Messages.Last().Id);
        
        var embedResponse = _responseBuilder
            .SetCurrentMessage(currentMessage)
            .SetBatchNumber(context.BatchNumber)
            .SetMessageCount(context.MessageCount)
            .SetFileCount(context.FileCount)
            .Build(ProgressStage.InProgress);

        await _interactionMessage.ModifyAsync(m => m.Embed = embedResponse);
        _lastMessage = currentMessage;
    }

    public async Task SendCompletedAsync(SocketInteractionContext interactionContext, BackupContext context)
    {
        var embedResponse = _responseBuilder
            .SetLastMessage(_lastMessage)
            .SetBatchNumber(context.BatchNumber)
            .SetMessageCount(context.MessageCount)
            .SetFileCount(context.FileCount)
            .Build(ProgressStage.Finished);

        await _interactionMessage.ModifyAsync(m => m.Embed = embedResponse);
        await GhostPing(interactionContext);
    }

    public async Task SendFailedAsync(SocketInteractionContext interactionContext, BackupContext context)
    {
        var embedResponse = _responseBuilder
            .SetBatchNumber(context.BatchNumber)
            .SetMessageCount(context.MessageCount)
            .SetFileCount(context.FileCount)
            .Build(ProgressStage.Failed);

        await _interactionMessage.ModifyAsync(m => m.Embed = embedResponse);
        await GhostPing(interactionContext);
    }

    public async Task SendInvalidAttemptAsync(SocketInteractionContext context, TimeSpan cooldownTime)
    {
        var formattedTime = cooldownTime > TimeSpan.FromHours(0.99)
            ? $"{cooldownTime.Hours} horas e {cooldownTime.Minutes} minutos"
            : $"{cooldownTime.Minutes} minutos e {cooldownTime.Seconds} segundos";

        await context.Interaction.FollowupAsync("Tentativa de backup inválida" +
                                                $"\n**{formattedTime}** restantes para poder efetuar o próximo backup");
        DelayedDeleteInteraction(context.Interaction);
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

    public async Task SendEmptyMessageBackupAsync(object? sender, BackupEventArgs args)
    {
        await _interactionMessage.ModifyAsync(m =>
        {
            m.Content = "*Tentativa de backup inválida: Não havia mensagens válidas para serem salvas*";
            m.Embed = null;
        });

        DelayedDeleteInteraction(args.InteractionContext.Interaction);
    }

    public async Task SendAlreadyInProgressAsync(SocketInteractionContext context)
    {
        await context.Interaction.ModifyOriginalResponseAsync(m => m.Content =
            "*Por limitações do Discord, não é possivel efetuar mais de um processo de backup simutaneamente*");

        DelayedDeleteInteraction(context.Interaction);
    }

    public async Task SendProcessCancelledAsync(object? sender, BackupEventArgs e)
    {
        await _interactionMessage.ModifyAsync(m =>
        {
            m.Content = "*O processo de backup foi cancelado*";
            m.Embed = null;
        });
    }

    public async Task SendProcessToCancelAsync(SocketInteractionContext context, bool noCurrentBackup = false)
    {
        await context.Interaction.ModifyOriginalResponseAsync(m => m.Content =
            "*O processo de backup foi cancelado com sucesso*");
        DelayedDeleteInteraction(context.Interaction);
    }

    private async Task GhostPing(SocketInteractionContext context)
    {
        var ping = await context.Channel.SendMessageAsync($"<@{context.User.Id}>");
        await Task.Delay(2000);
        await ping.DeleteAsync();
    }

    public async Task SendNoBackupInProgressAsync(SocketInteractionContext context)
    {
        await context.Interaction.ModifyOriginalResponseAsync(m => m.Content =
            "*Não há um backup em andamento para cancelar*");
        DelayedDeleteInteraction(context.Interaction);
    }

    public async Task SendForbiddenAsync(SocketInteractionContext context)
    {
        await context.Interaction.ModifyOriginalResponseAsync(m => m.Content =
            "*Você não possui as permissões adequadas para este comando*");
        DelayedDeleteInteraction(context.Interaction);
    }

    private void DelayedDeleteInteraction(SocketInteraction interaction)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(7000);
                await interaction.DeleteOriginalResponseAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                await LogFileWritter.LogError(ex, ex.Message);
            }
        });
    }
}