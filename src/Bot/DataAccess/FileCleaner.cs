// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using OPZBot.DataAccess.Models;

namespace OPZBot.DataAccess;

public class FileCleaner
{
    public async Task DeleteMessageFilesAsync(IEnumerable<Message> messages)
    {
        var concurrentDeletion = new List<Task>();
        foreach (var message in messages) concurrentDeletion.Add(DeleteMessage(message));

        var deletionInProgress = Task.WhenAll(concurrentDeletion);
        try
        {
            await deletionInProgress;
        }
        catch (Exception)
        {
            if (deletionInProgress.Exception is not null)
                throw deletionInProgress.Exception;
        }
    }

    private Task DeleteMessage(Message message)
    {
        if (message.File is null) return Task.CompletedTask;
        var filePath = $"{AppContext.BaseDirectory}{message.File}";
        if (Path.GetExtension(filePath) == string.Empty)
            if ((File.GetAttributes(filePath) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                Directory.Delete(filePath, true);
                return Task.CompletedTask;
            }

        File.Delete(filePath);
        return Task.CompletedTask;
    }
}