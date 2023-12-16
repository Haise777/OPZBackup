using Discord;

namespace OPZBot.Services.MessageBackup;

public record MessageDataBatchDto(IEnumerable<IUser> Users, IEnumerable<IMessage> Messages);