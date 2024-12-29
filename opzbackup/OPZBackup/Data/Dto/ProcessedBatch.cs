using OPZBackup.Data.Models;
using OPZBackup.FileManagement;

namespace OPZBackup.Data.Dto;

public record ProcessedBatch(
    IEnumerable<User> Users,
    IEnumerable<Message> Messages,
    IEnumerable<Downloadable> ToDownload
);