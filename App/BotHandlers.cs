﻿using Bot.Modules.BackupMessage;
using Bot.Utilities;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Bot
{
    internal class BotHandlers
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConsoleLogger _logger;

        public BotHandlers(IServiceScopeFactory serviceScopeFactory, ConsoleLogger logger)
        {
            _scopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task MyButtonHandler(SocketMessageComponent component)
        {
            await component.UpdateAsync(x => x.Content = "🖕");
        }

        public Task SlashCommandHandler(SocketSlashCommand command)
        {
            _ = Task.Run(async () =>
            {
                _logger.BotActions($"{command.User.Username}: {command.CommandName}");
                switch (command.Data.Name)
                {
                    case "backup":
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var bCommand = scope.ServiceProvider.GetRequiredService<BackupCommand>();
                            await bCommand.BackupOptions(command);
                        }
                        break;
                }

            }); return Task.CompletedTask;
        }
    }
}
