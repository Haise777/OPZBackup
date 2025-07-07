using OPZBackup.Services.Utils;
using Timer = OPZBackup.Services.Utils.Timer;

namespace OPZBackup.Services.Backup;

public class BackupPerformanceProfiler
{
    private readonly PerformanceProfiler _performanceProfiler;

    public BackupPerformanceProfiler(PerformanceProfiler performanceProfiler)
    {
        _performanceProfiler = performanceProfiler;

        _performanceProfiler.Subscribe("FetchTimer");
        _performanceProfiler.Subscribe("ProcessTimer");
        _performanceProfiler.Subscribe("SaveTimer");
        _performanceProfiler.Subscribe("DownloadTimer");
        _performanceProfiler.Subscribe("SaveMessages");
        _performanceProfiler.Subscribe("BatchTimer");
        _performanceProfiler.Subscribe("CompressionTimer");
    }

    public Timer FetchTimer => _performanceProfiler.Timers[nameof(FetchTimer)];
    public Timer ProcessTimer => _performanceProfiler.Timers[nameof(ProcessTimer)];
    public Timer SaveTimer => _performanceProfiler.Timers[nameof(SaveTimer)];
    public Timer DownloadTimer => _performanceProfiler.Timers[nameof(DownloadTimer)];
    public Timer SaveMessagesTimer => _performanceProfiler.Timers[nameof(SaveMessagesTimer)];
    public Timer BatchTimer => _performanceProfiler.Timers[nameof(BatchTimer)];
    public Timer CompressionTimer => _performanceProfiler.Timers[nameof(CompressionTimer)];
}