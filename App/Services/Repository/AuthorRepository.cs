using App.Services.Context;
using App.Services.Models;

namespace App.Services.Repository
{
    internal static class AuthorRepository
    {
        public static void SaveOnDatabase(List<Author> authors)
        {
            if (authors.Count == 0) return;
            var authorsToAdd = new List<Author>();

            using var context = new MessageBackupContext();

            foreach (var author in authors)
            {
                if (!context.Authors.Any(a => a.Id == author.Id))
                    authorsToAdd.Add(author);
            }



            context.Authors.AddRange(authorsToAdd);
            context.SaveChanges();
        }
    }
}
