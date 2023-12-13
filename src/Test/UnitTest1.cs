using Discord;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using OPZBot.DataAccess.Caching;
using OPZBot.DataAccess.Context;
using OPZBot.DataAccess.Models;
using OPZBot.Services;
using OPZBot.Services.MessageBackup;

namespace Test;

public class Tests
{
    private BackupService CUT;
    private Mock<IMessageFetcher> MessageFetcher;
    private Mock<IBackupMessageProcessor> BackupMsgProcessor;
    private Mock<IDiscordInteraction> SocketInteractionContext;
    private Mock<MyDbContext> _DbContext;
    private Mapper Mapper;
    private IdCacheManager CacheManager;
    
    [SetUp]
    public void Setup()
    {
        MessageFetcher = new Mock<IMessageFetcher>();
        BackupMsgProcessor = new Mock<IBackupMessageProcessor>();
        SocketInteractionContext = new Mock<IDiscordInteraction>();
        _DbContext = new Mock<MyDbContext>();

        var mockedContext = new List<User>().BuildMock().BuildMockDbSet();

        _DbContext.Setup(x => x.Users).Returns(mockedContext.Object);
        
        
        Mapper = new Mapper();
        CacheManager = new IdCacheManager();
        
        CUT = new(
            MessageFetcher.Object,
            Mapper,
            BackupMsgProcessor.Object,
            _DbContext.Object,
            CacheManager
            );
    }

    [Test]
    public void Test1()
    {
        
        
        Assert.Pass();
    }
}