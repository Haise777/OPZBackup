using Discord.Interactions;

namespace OPZBackup.ResponseHandlers;

public class InteractionState<T>
{
    public readonly SocketInteractionContext interactionContext;
    public readonly T data;
    public int CurrentPage { get; set; }
    
    public InteractionState(SocketInteractionContext interactionContext, T data)
    {
        this.data = data;
        this.interactionContext = interactionContext;
    }
}