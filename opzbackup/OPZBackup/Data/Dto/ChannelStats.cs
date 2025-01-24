using OPZBackup.Data.Models;

namespace OPZBackup.Data.Dto;

public record ChannelStats(
    int numberOfMessages,
    int numberOfFiles,
    ulong bytesize,
    IEnumerable<User> users
);