using Microsoft.Extensions.Configuration;
using OPZBackup.Exceptions;
using OPZBackup.FileManagement;
using Serilog;

namespace OPZBackup;

public static class Dev
{
    static Dev()
    {
#if DEBUG
        var configuration = App.GetConfigurationFromFile();

        TestGuildId = configuration.GetValue<ulong>($"app-dev:{nameof(TestGuildId)}");
        IsCleanRun = configuration.GetValue<bool>("app-dev:CleanRun");
        IsDebug = true;

#else
            TestGuildId = default(ulong);
            IsDebug = false;
            IsCleanRun = false;
#endif
    }

    public static bool IsDebug { get; }
    public static ulong TestGuildId { get; }
    public static bool IsCleanRun { get; }


    public static void DoCleanRun()
    {
        if (!IsCleanRun)
            throw new InvalidStartupException("Attempted to perform a clean run besides the flag being set to false");
        
        var prefix = "CLEAN RUN:";

        File.Delete("opzbackup.db");
        Log.Warning($"{prefix} database file deleted!");
        if (FileCleaner.DeleteDir("Backup"))
            Log.Warning($"{prefix} backup directory deleted!");
        if (FileCleaner.DeleteDir("logs"))
            Log.Warning($"{prefix} logs directory deleted!");
    }
}