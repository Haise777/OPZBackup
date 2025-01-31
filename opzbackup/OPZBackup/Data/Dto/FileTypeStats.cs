namespace OPZBackup.Data.Dto;

public record FileTypeStats(
    int NOfSentImages,
    int NOfSentVideos,
    int NOfSentAudios,
    int NOfSentOthers
);