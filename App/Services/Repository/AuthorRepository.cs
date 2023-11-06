using App.Services.Context;
using App.Services.Models;

namespace App.Services.Repository
{
    internal class AuthorBuilder
    {
        private ulong _id;
        private string _username;

        public AuthorBuilder(ulong id, string username)
        {
            _id = id;
            _username = username;
        }

        public void RegisterOnDatabase()
        {
            using var context = new MessageBackupContext();

            if (context.Users.Any(u => u.Id == _id))
            {
                return;
            }

            var newAuthor = new User()
            {
                Id = _id,
                Username = _username,
            };
            context.Users.Add(newAuthor);
            context.SaveChanges();
        }
    }
}
