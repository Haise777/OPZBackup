using Discord.Interactions;
using Serilog;

namespace OPZBackup.ResponseHandlers.Backup;

public class ModuleResponseHandler
{
    public async Task SendInvalidAttemptAsync(SocketInteractionContext context, TimeSpan cooldownTime)
    {
        var formattedTime = cooldownTime > TimeSpan.FromHours(0.99)
            ? $"{cooldownTime.Hours} horas e {cooldownTime.Minutes} minutos"
            : $"{cooldownTime.Minutes} minutos e {cooldownTime.Seconds} segundos";

        await context.Interaction.FollowupAsync("Tentativa de backup inválida" +
                                                $"\n**{formattedTime}** restantes para poder efetuar o próximo backup");
        DelayedDeleteInteraction(context);
    }

    // public async Task SendDeleteConfirmationAsync()
    // {
    //     var button = new ButtonBuilder()
    //         .WithStyle(ButtonStyle.Danger)
    //         .WithLabel("Confirmar")
    //         .WithCustomId(BackupInteractionModule.CONFIRM_USER_DELETE_ID);
    //     var buttonCancel = new ButtonBuilder()
    //         .WithStyle(ButtonStyle.Secondary)
    //         .WithLabel("Cancelar")
    //         .WithCustomId(BackupInteractionModule.CANCEL_USER_DELETE_ID);
    //
    //     var components = new ComponentBuilder()
    //         .WithButton(button)
    //         .WithButton(buttonCancel)
    //         .Build();
    //
    //     await _interactionContext.Interaction.RespondAsync(ephemeral: true, text:
    //         "**Todas as suas mensagens** junto de seu usuario serão apagados dos registros de backup permanentemente" +
    //         "\nDeseja prosseguir?", components: components);
    // }

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

    public async Task SendAlreadyInProgressAsync(SocketInteractionContext context)
    {
        await context.Interaction.ModifyOriginalResponseAsync(m => m.Content =
            "*Por limitações do Discord, não é possivel efetuar mais de um processo de backup simutaneamente*");

        DelayedDeleteInteraction(context);
    }

    public async Task SendProcessToCancelAsync(SocketInteractionContext context, bool noCurrentBackup = false)
    {
        await context.Interaction.ModifyOriginalResponseAsync(m => m.Content =
            "*O processo de backup foi cancelado com sucesso*");
        DelayedDeleteInteraction(context);
    }

    public async Task SendNoBackupInProgressAsync(SocketInteractionContext context)
    {
        await context.Interaction.ModifyOriginalResponseAsync(m => m.Content =
            "*Não há um backup em andamento para cancelar*");
        DelayedDeleteInteraction(context);
    }

    public async Task SendForbiddenAsync(SocketInteractionContext context)
    {
        await context.Interaction.ModifyOriginalResponseAsync(m => m.Content =
            "*Você não possui as permissões adequadas para este comando*");
        DelayedDeleteInteraction(context);
    }

    private void DelayedDeleteInteraction(SocketInteractionContext context)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(7000);
                await context.Interaction.DeleteOriginalResponseAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                //TODO await LogWritter.LogError(ex, ex.Message);
            }
        });
    }
}