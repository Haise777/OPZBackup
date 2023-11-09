using App.Services.Models;
using App.Services.Repository;
using Discord;
using Discord.WebSocket;

namespace App.Services;

internal class Backup
{
    private readonly List<Author> _authors = new();
    private readonly BackupRegisterRepository _backupRegRepository;
    private readonly DateTime _backupStartDate;
    private readonly List<Message> _messageBatch = new();
    private int _batchCounter = 1;

    private bool _firstBuild = true;
    private readonly ConsoleLogger _log = new(nameof(Backup));
    private Channel _selectedChannel;

    public Backup(ISocketMessageChannel channel, IUser commandAuthor)
    {
        _backupStartDate = CreateStartDate(DateTime.Now);
        _backupStartDate = _backupStartDate.AddMilliseconds(-_backupStartDate.Millisecond);
        _selectedChannel = new Channel { Name = channel.Name, Id = channel.Id };
        _backupRegRepository =
            new BackupRegisterRepository(_backupStartDate, commandAuthor.Id, _selectedChannel.Id);
        _authors.Add(new Author { Id = commandAuthor.Id, Username = commandAuthor.Username });
    }
    private DateTime CreateStartDate(DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0, dt.Kind);
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

    private void AddAuthorIfNotExists(IUser author)
    {
        if (!_authors.Any(a => a.Id == author.Id))
        {
            _authors.Add(new Author
            {
                Id = author.Id,
                Username = author.Username
            });
        }
    }

    public void Save()
    {
        if (_firstBuild)
        {
            SaveFirst();
            _firstBuild = false;
            return;
        }

        _log.BackupAction($"<!> Saving batch 'number {_batchCounter}'");

        AuthorRepository.SaveToDatabase(_authors);
        MessageRepository.SaveToDatabase(_messageBatch);
        _backupRegRepository.UpdateOnDatabase(_messageBatch.Last().Id);

        _log.BackupAction($"Finished batch 'number {_batchCounter}'");
        _log.BackupAction("Clearing message batch");
        _messageBatch.Clear();
        _batchCounter++;
    }

    private void SaveFirst()
    {
        _log.BackupAction("<!> Saving first batch");

        _selectedChannel = ChannelRepository.RegisterIfNotExists(_selectedChannel);
        AuthorRepository.SaveToDatabase(_authors);
        _backupRegRepository.CreateOnDatabase();
        MessageRepository.SaveToDatabase(_messageBatch);
        _backupRegRepository.InsertStartMessage(_messageBatch.First().Id);
        _backupRegRepository.UpdateOnDatabase(_messageBatch.Last().Id);

        _log.BackupAction("Finished first batch");
        _log.BackupAction("Clearing message batch");
        _messageBatch.Clear();
        _batchCounter++;
    }
}