using Discord.Interactions;

namespace OPZBackup.ResponseHandlers.Backup;

public class ServiceResponseHandlerFactory
{
    private readonly EmbedResponseFactory _embedResponseFactory;

    public ServiceResponseHandlerFactory(EmbedResponseFactory embedResponseFactory)
    {
        _embedResponseFactory = embedResponseFactory;
    }

    public ServiceResponseHandler Create(SocketInteractionContext interactionContext)
    {
        return new ServiceResponseHandler(interactionContext, _embedResponseFactory);
    }
}