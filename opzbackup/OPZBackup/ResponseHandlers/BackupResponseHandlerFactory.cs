using Discord.Interactions;
using OPZBackup.ResponseHandlers;

namespace OPZBackup.Modules;

public class BackupResponseHandlerFactory
{
    private readonly EmbedResponseBuilder _responseBuilder;
    
    public BackupResponseHandlerFactory(EmbedResponseBuilder responseBuilder)
    {
        _responseBuilder = responseBuilder;
    }
    
    public BackupResponseHandler Create(SocketInteractionContext interactionContext)
    {
        return new BackupResponseHandler(interactionContext, _responseBuilder);
    }
}