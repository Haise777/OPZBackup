namespace OPZBot.Utilities;

public delegate Task AsyncEventHandler<TArgs>(object? sender, TArgs args);
