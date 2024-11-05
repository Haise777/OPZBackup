// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using OPZBackup.Logger;
using OPZBackup.Services;
using Serilog;

namespace OPZBackup.ResponseHandlers;

public class BackupResponseHandler
{
    private readonly SocketInteractionContext _interactionContext;
    private readonly EmbedResponseBuilder _responseBuilder;
    private RestFollowupMessage? _interactionMessage;
    private IMessage? _lastMessage;
    
    //TODO-3 Make it so that it stores the SocketInteractionContext from the BackupModule
    public BackupResponseHandler(SocketInteractionContext interactionContext, EmbedResponseBuilder responseBuilder)
    {
        _interactionContext = interactionContext;
        _responseBuilder = responseBuilder;
    }
    
    //TODO-3 Add a method that gives a object that separates all of the EmbedResponse methods from the rest

    public async Task SendStartNotificationAsync(BackupContext context)
    {
        var embedResponse = _responseBuilder
            .SetAuthor(_interactionContext.User)
            .SetStartTime(DateTime.Now)
            .SetBatchNumber(context.BatchNumber)
            .SetMessageCount(context.MessageCount)
            .SetFileCount(context.FileCount)
            .Build(ProgressStage.Started);

        _interactionMessage = await _interactionContext.Interaction.FollowupAsync(embed: embedResponse);
    }

    public async Task SendBatchFinishedAsync(BackupContext context, BackupBatch batch)
    {
        if (_responseBuilder.StartMessage == null)
            _responseBuilder.SetStartMessage(
                await _interactionContext.Channel.GetMessageAsync(batch.Messages.First().Id));

        var currentMessage = await _interactionContext.Channel.GetMessageAsync(batch.Messages.Last().Id);
        
        var embedResponse = _responseBuilder
            .SetCurrentMessage(currentMessage)
            .SetBatchNumber(context.BatchNumber)
            .SetMessageCount(context.MessageCount)
            .SetFileCount(context.FileCount)
            .Build(ProgressStage.InProgress);

        await _interactionMessage.ModifyAsync(m => m.Embed = embedResponse);
        _lastMessage = currentMessage;
    }

    public async Task SendCompletedAsync(BackupContext context)
    {
        var embedResponse = _responseBuilder
            .SetLastMessage(_lastMessage)
            .SetBatchNumber(context.BatchNumber)
            .SetMessageCount(context.MessageCount)
            .SetFileCount(context.FileCount)
            .Build(ProgressStage.Finished);

        await _interactionMessage.ModifyAsync(m => m.Embed = embedResponse);
        await GhostPing();
    }

    public async Task SendFailedAsync(BackupContext context)
    {
        var embedResponse = _responseBuilder
            .SetBatchNumber(context.BatchNumber)
            .SetMessageCount(context.MessageCount)
            .SetFileCount(context.FileCount)
            .Build(ProgressStage.Failed);

        await _interactionMessage.ModifyAsync(m => m.Embed = embedResponse);
        await GhostPing();
    }

    public async Task SendInvalidAttemptAsync(TimeSpan cooldownTime)
    {
        var formattedTime = cooldownTime > TimeSpan.FromHours(0.99)
            ? $"{cooldownTime.Hours} horas e {cooldownTime.Minutes} minutos"
            : $"{cooldownTime.Minutes} minutos e {cooldownTime.Seconds} segundos";

        await _interactionContext.Interaction.FollowupAsync("Tentativa de backup inválida" +
                                                $"\n**{formattedTime}** restantes para poder efetuar o próximo backup");
        DelayedDeleteInteraction();
    }

    public async Task SendDeleteConfirmationAsync()
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

        await _interactionContext.Interaction.RespondAsync(ephemeral: true, text:
            "**Todas as suas mensagens** junto de seu usuario serão apagados dos registros de backup permanentemente" +
            "\nDeseja prosseguir?", components: components);
    }

    public async Task SendUserDeletionResultAsync(bool wasDeleted)
    {
        if (wasDeleted)
        {
            await _interactionContext.Interaction.DeferAsync();
            await _interactionContext.Interaction.DeleteOriginalResponseAsync();
            await _interactionContext.Interaction.FollowupAsync(
                $"***{_interactionContext.User.Username}** foi deletado dos registros de backup*");
            return;
        }

        await _interactionContext.Interaction.DeferAsync();
        await _interactionContext.Interaction.DeleteOriginalResponseAsync();
    }

    public async Task SendEmptyMessageBackupAsync()
    {
        await _interactionMessage.ModifyAsync(m =>
        {
            m.Content = "*Tentativa de backup inválida: Não havia mensagens válidas para serem salvas*";
            m.Embed = null;
        });

        DelayedDeleteInteraction();
    }

    public async Task SendAlreadyInProgressAsync()
    {
        await _interactionContext.Interaction.ModifyOriginalResponseAsync(m => m.Content =
            "*Por limitações do Discord, não é possivel efetuar mais de um processo de backup simutaneamente*");

        DelayedDeleteInteraction();
    }

    public async Task SendProcessCancelledAsync()
    {
        await _interactionMessage.ModifyAsync(m =>
        {
            m.Content = "*O processo de backup foi cancelado*";
            m.Embed = null;
        });
    }

    public async Task SendProcessToCancelAsync(bool noCurrentBackup = false)
    {
        await _interactionContext.Interaction.ModifyOriginalResponseAsync(m => m.Content =
            "*O processo de backup foi cancelado com sucesso*");
        DelayedDeleteInteraction();
    }

    private async Task GhostPing()
    {
        var ping = await _interactionContext.Channel.SendMessageAsync($"<@{_interactionContext.User.Id}>");
        await Task.Delay(2000);
        await ping.DeleteAsync();
    }

    public async Task SendNoBackupInProgressAsync()
    {
        await _interactionContext.Interaction.ModifyOriginalResponseAsync(m => m.Content =
            "*Não há um backup em andamento para cancelar*");
        DelayedDeleteInteraction();
    }

    public async Task SendForbiddenAsync()
    {
        await _interactionContext.Interaction.ModifyOriginalResponseAsync(m => m.Content =
            "*Você não possui as permissões adequadas para este comando*");
        DelayedDeleteInteraction();
    }

    private void DelayedDeleteInteraction()
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(7000);
                await _interactionContext.Interaction.DeleteOriginalResponseAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                await LogWritter.LogError(ex, ex.Message);
            }
        });
    }
}