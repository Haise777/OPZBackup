namespace OPZBot.Utilities;

public delegate Task AsyncEventHandler<in TArgs>(object? sender, TArgs args);