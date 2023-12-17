using Discord.Interactions;
using OPZBot.DataAccess.Models;

namespace OPZBot.Services.MessageBackup;

public class BackupEventArgs : EventArgs
{
    public SocketInteractionContext? InteractionContext { get; set; }
    public BackupRegistry? Registry { get; set; }
    public MessageDataBatchDto? MessageBatch { get; set; }

    public BackupEventArgs(SocketInteractionContext? context = null, BackupRegistry? registry = null,
        MessageDataBatchDto? messageBatch = null)
    {
        InteractionContext = context;
        Registry = registry;
        MessageBatch = messageBatch;
    }
}