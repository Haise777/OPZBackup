// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

namespace OPZBot.Logging;

public static class LogFileWritter
{
    public static async Task LogHostCrash(Exception ex)
    {
        var sessionDate = $"{Program.SessionTime:dd.MM.yyyy_HH.mm.ss}";
        await using var sw = new StreamWriter(Path.Combine(AppContext.BaseDirectory,
            $"crashreport_{sessionDate}.log"));

        await sw.WriteLineAsync("Host terminated with error:\n" + ex);
    }

    public static async Task LogError(Exception ex, string? message)
    {
        if (!Directory.Exists(Path.Combine(AppContext.BaseDirectory, "errorlogs")))
            Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "errorlogs"));

        var sessionDate = $"{Program.SessionTime:dd.MM.yyyy_HH.mm.ss}";

        await using var fileWriter = new StreamWriter(
            Path.Combine(AppContext.BaseDirectory, $"errorlogs/log_{sessionDate}.log"), true);

        await fileWriter.WriteLineAsync($"{DateTime.Now}\n{message}\n{ex}\n\n");
    }
}