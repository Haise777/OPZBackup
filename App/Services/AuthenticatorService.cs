using Discord;
using Discord.WebSocket;

namespace Bot.Services
{
    internal static class AuthenticatorService
    {
        private static ulong _starRole;
        public static bool IsAuthorized(IUser user)
        {

            var author = (SocketGuildUser)user;

            if (author.Roles.Any(r => r.Id == _starRole))
            {
                return true;
            }

            return false;
        }

        public static void SetStarRole(ulong starRoleId)
        {
            if (_starRole != default)
                throw new InvalidOperationException("StarRole value was already set");

            _starRole = starRoleId;
        }
    }
}
