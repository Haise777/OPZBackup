using App.Services.Models;
using App.Services.Repository;
using Discord;
using Discord.WebSocket;

namespace App.Services
{
    internal class BackupBuilder
    {
        private Channel _selectedChannel;
        private AuthorRepository _authorRepository;
        private BackupRepository _backupRepository;
        private List<IMessage> _messageBatch;
        private bool firstBuild = true;

        public BackupBuilder(ISocketMessageChannel channel, IUser commandAuthor)
        {
            _selectedChannel = ChannelRepository.RegisterIfNotExists(channel);
            _authorRepository = new AuthorRepository();
            _authorRepository.RegisterIfNotExists(commandAuthor);
            _backupRepository = new BackupRepository(DateTime.Now, commandAuthor.Id, _selectedChannel.Id);
        }

        public void AddMessage(IMessage message)
        {
            _messageBatch.Add(message);
        }

        public void Build()
        {
            if (firstBuild)
            {
                FirstBuild();
                firstBuild = false;
            }

            SaveBuild();
        }

        private void SaveBuild()
        {

        }

        private void FirstBuild()
        {
            _authorRepository.SaveOnDatabase();
            _backupRepository.
        }




    }
}
