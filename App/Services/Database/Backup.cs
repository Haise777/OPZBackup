using App.Services.Database.Models;
using App.Services.Database.Repository;
using App.Utilities;
using Discord;
using Discord.WebSocket;

namespace App.Services.Database;

internal class Backup
{
    private readonly ConsoleLogger _log = new(nameof(Backup));
    private readonly BackupRegisterRepository _backupRegRepository;
    private readonly DateTime _backupStartDate;
    private readonly List<Author> _authors = new();
    private readonly List<Message> _messageBatch = new();
    private readonly Channel _selectedChannel;

    private int _batchCounter = 1;
    private bool _firstSave = true;

    public Backup(ISocketMessageChannel channel, IUser commandAuthor)
    {
        _backupStartDate = DateTime.Now.WithoutMilliseconds();
        _selectedChannel = new Channel { Name = channel.Name, Id = channel.Id };
        _backupRegRepository = new BackupRegisterRepository(_backupStartDate, commandAuthor.Id, _selectedChannel.Id);
        _authors.Add(new Author { Id = commandAuthor.Id, Username = commandAuthor.Username });
    }

    public void AddMessage(IMessage message)
    {
        AddAuthorIfNotExists(message.Author);

        _messageBatch.Add(new Message
        {
            Id = message.Id,
            AuthorId = message.Author.Id,
            Date = message.Timestamp.DateTime,
            EditDate = message.EditedTimestamp.HasValue ? message.EditedTimestamp.Value.DateTime : null,
            Content = message.Content,
            ChannelId = message.Channel.Id,
            BackupDate = _backupStartDate
        });
    }

    public void Save()
    {
        if (_firstSave)
        {
            FirstTimeSave();
            _firstSave = false;
            return;
        }

        _log.BackupAction($"<!> Saving batch 'number {_batchCounter}'");
        AuthorRepository.SaveNewToDatabase(_authors);
        MessageRepository.SaveToDatabase(_messageBatch);
        _backupRegRepository.UpdateOnDatabase(_messageBatch[^1].Id);
        _log.BackupAction($"Finished batch 'number {_batchCounter}'");

        _messageBatch.Clear();
        _batchCounter++;
    }

    private void FirstTimeSave()
    {
        _log.BackupAction("<!> Saving first batch");
        ChannelRepository.RegisterIfNotExists(_selectedChannel);
        AuthorRepository.SaveNewToDatabase(_authors);
        _backupRegRepository.CreateOnDatabase();
        MessageRepository.SaveToDatabase(_messageBatch);
        _backupRegRepository.InsertStartMessage(_messageBatch[0].Id);
        _backupRegRepository.UpdateOnDatabase(_messageBatch[^1].Id);
        _log.BackupAction("Finished first batch");

        _messageBatch.Clear();
        _batchCounter++;
    }

    private void AddAuthorIfNotExists(IUser author)
    {
        if (!_authors.Exists(a => a.Id == author.Id))
        {
            _authors.Add(new Author
            {
                Id = author.Id,
                Username = author.Username
            });
        }
    }
}