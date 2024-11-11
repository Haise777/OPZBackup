using Discord.Interactions;

namespace OPZBackup.ResponseHandlers.Backup;

public class ServiceResponseHandlerFactory
{
    private readonly ResponseBuilder _responseBuilder;

    public ServiceResponseHandlerFactory(ResponseBuilder responseBuilder)
    {
        _responseBuilder = responseBuilder;
    }

    public ServiceResponseHandler Create(SocketInteractionContext interactionContext)
    {
        return new ServiceResponseHandler(interactionContext, _responseBuilder);
    }
}