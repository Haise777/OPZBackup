using Bot.Services.Database.Context;
using Bot.Services.Database.Models;
using Bot.Utilities;
using Discord;

namespace Bot.Services.Database.Repository
{
    internal class AuthorRepository
    {
        private readonly ConsoleLogger _log = new(nameof(AuthorRepository));
        private readonly MessageBackupContext _backupContext;

        public AuthorRepository(DbConnection dbConnection)
        {
            _backupContext = dbConnection.GetConnection();
        }

        public void SaveNewToDatabase(List<Author> authors)
        {
            var authorsToAdd = new List<Author>();

            foreach (var author in authors)
            {
                if (!_backupContext.Authors.Any(a => a.Id == author.Id))
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
            try
            {
                _backupContext.Authors.AddRange(authorsToAdd);
                _backupContext.SaveChanges();
                _log.BackupAction("Saved new authors to the database");
            }
            catch (Exception ex)
            {
                _log.Exception("Failed to save new authors to database", ex);
                throw;
            }
        }

        public void DeleteAuthor(IUser author) //TODO: Smallchagnes
        {
            var authorToDelete = _backupContext.Authors.SingleOrDefault(a => a.Id == author.Id);
            if (authorToDelete is null)
                throw new InvalidOperationException("Author to delete not found on database");

            _backupContext.Authors.Remove(authorToDelete);
            _backupContext.SaveChanges();

            _log.BackupAction($"All messages and user record deleted: {author.Username}");
        }
    }
}
