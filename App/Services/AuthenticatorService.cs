using Discord;
using Discord.WebSocket;

namespace Bot.Services
{
    internal static class AuthenticatorService
    {
        public static bool IsAuthorized(IUser user)
        {
            var author = (SocketGuildUser)user;

            if (author.Roles.Any(r => r.Id == Program.StarRole))
            {
                return true;
            }

            return false;
        }
    }
}
