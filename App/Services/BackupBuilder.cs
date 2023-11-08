using App.Services.Models;
using App.Services.Repository;
using Discord;
using Discord.WebSocket;

namespace App.Services
{
    internal class BackupBuilder
    {
        private Channel _selectedChannel;
        private BackupRegisterRepository _backupRegRepository;
        private DateTime _backupStartDate;
        private List<Author> _authors;
        private List<Message> _messageBatch;
        private bool firstBuild = true;

        public BackupBuilder(ISocketMessageChannel channel, IUser commandAuthor)
        {
            _backupStartDate = DateTime.Now;
            _selectedChannel = new Channel() { Name = channel.Name, Id = channel.Id };
            _backupRegRepository = new BackupRegisterRepository(_backupStartDate, commandAuthor.Id, _selectedChannel.Id);
            _authors = new List<Author>()
            {
                new Author() { Id = commandAuthor.Id, Username = commandAuthor.Username }
            };
        }


        public void AddMessage(IMessage message)
        {
            AddAuthorIfNotExists(message.Author);

            _messageBatch.Add(new Message()
            {
                Id = message.Id,
                Author = message.Author.Id,
                Date = message.Timestamp.DateTime,
                EditDate = message.EditedTimestamp.HasValue ? message.EditedTimestamp.Value.DateTime : null,
                Content = message.Content,
                ChannelId = message.Channel.Id,
                BackupDate = _backupStartDate,
            });
        }

        private void AddAuthorIfNotExists(IUser author)
        {
            if (!_authors.Any(a => a.Id == author.Id))
                _authors.Add(new Author()
                {
                    Id = author.Id,
                    Username = author.Username
                });
        }

        public void Save()
        {
            if (firstBuild)
            {
                SaveFirst();
                firstBuild = false;
            }

            AuthorRepository.SaveOnDatabase(_authors);
            MessageRepository.SaveToDatabase(_messageBatch);
            _backupRegRepository.UpdateOnDatabase(_messageBatch.Last().Id);

            _messageBatch.Clear();
        }



        private void SaveFirst()
        {
            _selectedChannel = ChannelRepository.RegisterIfNotExists(_selectedChannel);
            AuthorRepository.SaveOnDatabase(_authors);
            _backupRegRepository.CreateOnDatabase();
            MessageRepository.SaveToDatabase(_messageBatch);
            _backupRegRepository.InsertStartMessage(_messageBatch.First().Id);
            _backupRegRepository.UpdateOnDatabase(_messageBatch.Last().Id);

            _messageBatch.Clear();
        }




    }
}
