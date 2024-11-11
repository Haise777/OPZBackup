using Microsoft.Extensions.Configuration;
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


    public static async Task DoCleanRun()
    {
        Log.Warning("!! CLEAN RUN flag is set to true");
        var prefix = "CLEAN RUN:";

        File.Delete("opzbackup.db");
        Log.Warning($"{prefix} database file deleted!");
        if (await FileCleaner.DeleteDirAsync("Backup"))
            Log.Warning($"{prefix} backup directory deleted!");
    }
}