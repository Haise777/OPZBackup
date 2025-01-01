using OPZBackup.Services.Backup;

namespace OPZBackup.Logger;

public class BackupLoggerFactory
{
    public BackupLogger Create(BackupContext context) 
        => new(context);
}