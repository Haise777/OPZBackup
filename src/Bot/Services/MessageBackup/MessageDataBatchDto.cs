using Discord;
using OPZBot.DataAccess.Models;

namespace OPZBot.Services.MessageBackup;

public record MessageDataBatchDto(IEnumerable<User> Users, IEnumerable<Message> Messages);