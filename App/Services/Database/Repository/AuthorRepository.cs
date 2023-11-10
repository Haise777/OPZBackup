﻿using App.Services.Database.Models;
using App.Utilities;
using Discord;

namespace App.Services.Database.Repository
{
    internal static class AuthorRepository
    {
        private readonly static ConsoleLogger _log = new(nameof(AuthorRepository));

        public static void SaveNewToDatabase(List<Author> authors)
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

        public static void DeleteAuthor(IUser author)
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
