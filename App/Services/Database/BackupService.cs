using Bot.Services.Database.Models;
using Bot.Services.Database.Repository;
using Discord;

namespace Bot.Services.Database
{
    internal class BackupService
    {
        private readonly AuthorRepository _authorRepository;
        private readonly MessageRepository _messageRepository;
        private readonly BackupRegisterRepository _backupRegisterRepository;
        private readonly ChannelRepository _channelRepository;

        public BackupService(AuthorRepository ar, MessageRepository mr, BackupRegisterRepository brr, ChannelRepository cr)
        {
            _authorRepository = ar;
            _messageRepository = mr;
            _backupRegisterRepository = brr;
            _channelRepository = cr;
        }

        public void StandardBackup(List<Author> authors, List<Message> messageBatch)
        {
            _authorRepository.SaveNewToDatabase(authors);
            _messageRepository.SaveToDatabase(messageBatch);
        }

        public void FirstTimeBackup(BackupRegister backupRegister, Channel selectedChannel, List<Author> authors, List<Message> messageBatch)
        {
            _channelRepository.RegisterIfNotExists(selectedChannel);
            _authorRepository.SaveNewToDatabase(authors);
            _backupRegisterRepository.CreateOnDatabase(backupRegister);
            _messageRepository.SaveToDatabase(messageBatch);
        }

        public void UpdateRegisterOnDatabase(BackupRegister backupRegister)
            => _backupRegisterRepository.UpdateOnDatabase(backupRegister);

        public void DeleteAuthor(IUser author)
            => _authorRepository.DeleteAuthor(author);
    }
}
