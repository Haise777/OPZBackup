// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OPZBot.DataAccess;
using OPZBot.DataAccess.Context;
using OPZBot.Logging;

namespace OPZBot.Modules;

[Group("blacklist", "gerenciar blacklist")]
public class BlacklistInteractionModule(
    MyDbContext _dataContext,
    Mapper _mapper,
    ILogger<BlacklistInteractionModule> _logger)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("lista", "lista todos os usuarios inseridos na blacklist")]
    public async Task ListAll()
    {
        _logger.LogCommandExecution(
            "Blacklist", Context.User.Username, Context.Channel.Name, nameof(ListAll));
        
        await DeferAsync();
        
        var blacklisteds = await _dataContext.Users
            .Where(u => u.IsBlackListed == true)
            .Select(u => u.Username)
            .ToArrayAsync();

        if (blacklisteds.Length == 0)
        {
            await ModifyOriginalResponseAsync(m =>
                m.Content = "*Não há nenhum usuario atualmente na blacklist*");
            await Task.Delay(7000);
            await DeleteOriginalResponseAsync();
            return;
        }

        await ModifyOriginalResponseAsync(m =>
        {
            foreach (var blacklisted in blacklisteds)
            {
                m.Content += $"- {blacklisted}\n";
            }
        });
    }
    
    [SlashCommand("remover", "remove um usuario da blacklist")]
    public async Task RemoveFrom(SocketUser usuario)
    {
        _logger.LogCommandExecution(
            "Blacklist", Context.User.Username, Context.Channel.Name, nameof(RemoveFrom));
        
        await DeferAsync();
        if (!await UserHasPermission()) return;
        if (await IsBackupInProgress()) return;
        
        var user = await _dataContext.Users
            .SingleOrDefaultAsync(e => e.Id == usuario.Id);

        if (user is null || !user.IsBlackListed)
        {
            _logger.LogInformation("{service}: There was no matched user in the blacklist", "Backup");
            await ModifyOriginalResponseAsync(m =>
                m.Content = "*Usuario não existente na blacklist*");
            await DelayedDeleteInteraction();
            return;
        }
        
        try
        {
            user.IsBlackListed = false;
            await _dataContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            await ModifyOriginalResponseAsync(m =>
                m.Content = "*Ocorreu um erro ao tentar remover o usuario da blacklist*");
            await DelayedDeleteInteraction();
            throw;
        }
        _logger.LogInformation("{service}: User '{user}' was removed from blacklist", "Backup", user.Username);
        await ModifyOriginalResponseAsync(m =>
            m.Content = $"*{user.Username} foi removido da blacklist*");
    }

    [SlashCommand("adicionar", "adiciona um usuario na blacklist")]
    public async Task AddToBlacklist(SocketUser usuario)
    {
        _logger.LogCommandExecution(
            "Blacklist", Context.User.Username, Context.Channel.Name, nameof(AddToBlacklist));
        
        await DeferAsync();
        if (!await UserHasPermission()) return;
        if (await IsBackupInProgress()) return;
        
        var user = await _dataContext.Users.SingleOrDefaultAsync(u => u.Id == usuario.Id);
        if (user is null)
        {
            user = _mapper.Map(usuario);
            _dataContext.Users.Add(user);
        }
        else if (user.IsBlackListed)
        {
            _logger.LogInformation("{service}: User '{user}' was already added to blacklist", "Backup", user.Username);
            await ModifyOriginalResponseAsync(m =>
                m.Content = "*Usuario já adicionado à blacklist*");
            await DelayedDeleteInteraction();
            return;
        }

        try
        {
            user.IsBlackListed = true;
            await _dataContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            await ModifyOriginalResponseAsync(m =>
                m.Content = "*Ocorreu um erro ao tentar adicionar usuario à blacklist*");
            await DelayedDeleteInteraction();
            throw;
        }

        _logger.LogInformation("{service}: User '{user}' was added to blacklist", "Backup", user.Username);
        await ModifyOriginalResponseAsync(m =>
            m.Content = $"*{user.Username} foi adicionado(a) à blacklist*");
    }

    private async Task DelayedDeleteInteraction()
    {
        await Task.Delay(7000);
        await DeleteOriginalResponseAsync();
    }
    
    private async Task<bool> UserHasPermission()
    {
        var user = Context.User as SocketGuildUser;

        if (user!.Roles.Any(x => x.Id == Program.MainAdminRoleId))
            return true;

        await ModifyOriginalResponseAsync(m =>
            m.Content = "*Você não tem permissões necessárias para executar este comando*");
        await DelayedDeleteInteraction();
        return false;
    }
    
    private async Task<bool> IsBackupInProgress()
    {
        if (!BackupInteractionModule.IsBackupInProgress) return false;
        
        _logger.LogInformation("{service}: Can't alter blacklist while a backup process is running", "Backup");
        await ModifyOriginalResponseAsync(m =>
            m.Content = "*Não é possivel alterar a blacklist enquanto há um backup em andamento*");
        await DelayedDeleteInteraction();
        return true;
    }
}