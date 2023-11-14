using Bot.Services.Database.Models;
using Bot.Utilities;
using Discord;

namespace Bot.Services.Database.Repository
{
    internal class AuthorRepository
    {
        private readonly ConsoleLogger _log = new(nameof(AuthorRepository));
        private readonly DbConnection _connection;

        public AuthorRepository(DbConnection dbConnection)
        {
            _connection = dbConnection;
        }

        public void SaveNewToDatabase(List<Author> authors)
        {
            var context = _connection.GetConnection();
            var authorsToAdd = new List<Author>();

            foreach (var author in authors)
            {
                if (!context.Authors.Any(a => a.Id == author.Id))
                {
                    authorsToAdd.Add(author);
                    _log.BackupAction($"New author to add: '{author.Username}'");
                }
            }

            if (authorsToAdd.Count == 0)
            {
                _log.BackupAction("No authors to add");
                return;
            }

            context.Authors.AddRange(authorsToAdd);
            context.SaveChanges();
            _log.BackupAction("Saved new authors to the database");
        }

        public void DeleteAuthor(IUser author)
        {
            var context = _connection.GetConnection();
            var authorToDelete = context.Authors.SingleOrDefault(a => a.Id == author.Id);
            if (authorToDelete is null) return;

            context.Authors.Remove(authorToDelete);
            context.SaveChanges();

            _log.BackupAction($"All messages and user record deleted: {author.Username}");
        }

        public bool CheckIfExists(IUser author)
        {
            var context = _connection.GetConnection();
            return context.Authors.Any(a => a.Id == author.Id);
        }
    }
}
