﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using OPZBackup.Data;
using OPZBackup.Exceptions;
using OPZBackup.FileManagement;
using OPZBackup.Modules;
using OPZBackup.ResponseHandlers;
using OPZBackup.Services.Utils;

namespace OPZBackup.Services;

public class BackupService
{
    private readonly MessageFetcher _messageFetcher;
    private readonly MessageProcessor _messageProcessor;
    private readonly BackupContextFactory _contextFactory;
    private readonly AttachmentDownloader _attachmentDownloader;
    private readonly MyDbContext _dbContext;
    private BackupResponseHandler? _responseHandler;
    private BackupContext _context;
    private bool _forcedStop;

    public BackupService(MessageFetcher messageFetcher,
        MessageProcessor messageProcessor,
        BackupContextFactory contextFactory,
        MyDbContext dbContext,
        AttachmentDownloader attachmentDownloader)
    {
        _messageFetcher = messageFetcher;
        _messageProcessor = messageProcessor;
        _contextFactory = contextFactory;
        _dbContext = dbContext;
        _attachmentDownloader = attachmentDownloader;
    }

    public async Task StartBackupAsync(SocketInteractionContext interactionContext, BackupResponseHandler responseHandler, bool isUntilLast)
    {
        _responseHandler = responseHandler;
        _context = await _contextFactory.RegisterNewBackup(interactionContext, isUntilLast);
        
        try
        {
            await _responseHandler.SendStartNotificationAsync(_context);
            await BackupMessages();
            await CompressFilesAsync();
        }
        catch (Exception ex)
        {
            await _responseHandler.SendFailedAsync(_context);
            await _context.RollbackAsync();
            throw;
        }
        
        await _responseHandler.SendCompletedAsync(_context);
    }

    private async Task CompressFilesAsync()
    {
        //TODO-1 Implements file compression after the backup is finished
        throw new NotImplementedException();
    }

    private async Task BackupMessages()
    {
        ulong lastMessageId = 0;

        while (true)
        {
            if (_forcedStop)
                throw new BackupCanceledException();
            if (_context.IsStopped) //Reached the last backuped message
                break;

            var fetchedMessages = await FetchMessages(lastMessageId);

            if (!fetchedMessages.Any()) //Reached the end of channel
                break;

            var backupBatch = await _messageProcessor.ProcessAsync(fetchedMessages, _context);

            lastMessageId = fetchedMessages.Last().Id;

            if (!backupBatch.Messages.Any())
                continue;

            await SaveBatch(backupBatch);

            _context.MessageCount += backupBatch.Messages.Count();
            _context.BatchNumber++;
            await _responseHandler.SendBatchFinishedAsync(_context, backupBatch);
        }
    }

    private async Task SaveBatch(BackupBatch batch)
    {
        _dbContext.Messages.AddRange(batch.Messages);

        if (batch.Users.Any())
            _dbContext.Users.AddRange(batch.Users);

        await _dbContext.SaveChangesAsync();

        if (batch.ToDownload.Any())
            await DownloadMessageAttachments(batch.ToDownload);
    }

    private async Task DownloadMessageAttachments(IEnumerable<Downloadable> toDownload)
    {
        foreach (var downloadable in toDownload)
        {
            _context.FileCount += downloadable.Attachments.Count();
            foreach (var attachment in downloadable.Attachments)
                _context.SavedFilePaths.Add(attachment.GetFullPath());
        }

        await _attachmentDownloader.DownloadAsync(toDownload);
    }

    private async Task<IEnumerable<IMessage>> FetchMessages(ulong lastMessageId)
    {
        var channelContext = _context.InteractionContext.Channel;
        
        if (lastMessageId == 0)
            return await _messageFetcher.FetchAsync(channelContext);
        else
            return await _messageFetcher.FetchAsync(channelContext, lastMessageId);
    }

    public void Cancel()
    {
        _forcedStop = true;
    }
}