using App.Services.Context;
using App.Services.Models;
using Discord;

namespace App.Services.Repository
{
    internal class AuthorRepository
    {
        private List<Author> _authors = new List<Author>();


        public static bool IsRegistered(ulong id)
        {
            var context = new MessageBackupContext();

            return context.Authors.Any(u => u.Id == id);
        }

        public void AddAuthor(IUser author)
        {
            _authors.Add(
                new Author
                {
                    Id = author.Id,
                    Username = author.Username
                });
        }


    }
}
