using App.Services.Database.Models;

namespace App.Services.Database.Repository
{
    internal static class AuthorRepository
    {
        public static void SaveToDatabase(List<Author> authors)
        {
            var _log = new ConsoleLogger(nameof(AuthorRepository));

            var authorsToAdd = new List<Author>();
            var context = DbConnection.GetConnection();

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
                ConsoleLogger.GenericException($"{nameof(AuthorRepository)}-{nameof(SaveToDatabase)}", ex);
                throw;
            }
        }
    }
}
