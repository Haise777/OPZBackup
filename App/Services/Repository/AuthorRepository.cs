using App.Services.Context;
using App.Services.Models;
using Discord;

namespace App.Services.Repository
{
    internal class AuthorRepository
    {
        private List<Author> _authors = new List<Author>();


        public void RegisterIfNotExists(IUser author)
        {
            if (_authors.Any(a => a.Id == author.Id))
                return;

            using var context = new MessageBackupContext();

            if (context.Authors.Any(a => a.Id == author.Id))
                return;

            AddAuthor(author);
        }


        public void SaveOnDatabase()
        {
            if (_authors.Count == 0) return;

            using var context = new MessageBackupContext();

            context.Authors.AddRange(_authors);
            context.SaveChanges();
            _authors.Clear();
        }


        private void AddAuthor(IUser author)
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
