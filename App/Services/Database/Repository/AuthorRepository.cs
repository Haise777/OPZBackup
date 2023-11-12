using Bot.Services.Database.Models;
using Bot.Utilities;
using Discord;

namespace Bot.Services.Database.Repository
{
    internal class AuthorRepository
    {
        private readonly ConsoleLogger _log = new(nameof(AuthorRepository));

        public void SaveNewToDatabase(List<Author> authors)
        {
            var context = DbConnection.GetConnection();
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
            try
            {
                context.Authors.AddRange(authorsToAdd);
                context.SaveChanges();
                _log.BackupAction("Saved new authors to the database");
            }
            catch (Exception ex)
            {
                _log.Exception("Failed to save new authors to database", ex);
                throw;
            }
        }

        public void DeleteAuthor(IUser author)
        {
            var context = DbConnection.GetConnection();

            var authorToDelete = context.Authors.SingleOrDefault(a => a.Id == author.Id);
            if (authorToDelete is null)
                throw new InvalidOperationException("Author to delete not found on database");

            context.Authors.Remove(authorToDelete);
            context.SaveChanges();

            _log.BackupAction($"All messages and user record deleted: {author.Username}");
        }
    }
}
