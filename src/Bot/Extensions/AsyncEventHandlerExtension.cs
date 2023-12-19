using OPZBot.Utilities;

namespace OPZBot.Extensions;

public static class AsyncEventHandlerExtension
{
    public static Task InvokeAsync<TArgs>(this AsyncEventHandler<TArgs>? eventAsync, object? sender, TArgs e)
    {
        return eventAsync is null
            ? Task.CompletedTask
            : Task.WhenAll(eventAsync.GetInvocationList()
                .Cast<AsyncEventHandler<TArgs>>()
                .Select(f => f(sender, e)));
    }
}