using OPZBackup.Data.Models;

namespace OPZBackup.Data.Dto;
// Show a embed with all of the above, plus
// total num of mention to other users with then number of mention for each individual user
// each file type sent with their number of sent
// like: image: 20, video: 7, audio: 2, others: 34
// top most common words sent inside a message
// active period

public record UserStats(
    User? user,
    KeyValuePair<ulong, int>[][] NumberOfMentions,
    KeyValuePair<string, int>[][] MostCommonWords,
    FileTypeStats fileTypeStats
);

//TODO: Temporary solution
public record UserStatsWithUsernames(
    User? user,
    KeyValuePair<string, int>[][] NumberOfMentions,
    KeyValuePair<string, int>[][] MostCommonWords,
    FileTypeStats fileTypeStats
);