using Discord;
using Discord.Interactions;

namespace OPZBot.Services.MessageBackup;

public class BackupResponseBuilder
{
    //TODO Fix DateTime using wrong timezones
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public IMessage? StartMessage { get; set; }
    public IMessage? LastMessage { get; set; }
    public IUser? Author { get; set; }
    public int NumberOfMessages; //TODO This probably a SRP violation
    public int BatchNumber; //And also this

    public Embed Build(BackupStage stage)
    {
        var embedBuilder = ConstructEmbed();
        var t = (DateTime.Now - StartTime)!.Value;
        var elapsed = $"{t.TotalHours:00}:{t:mm\\:ss}";

        switch (stage)
        {
            case BackupStage.Started:
                embedBuilder
                    .WithTitle("Em progresso...")
                    .WithColor(Color.Gold)
                    .AddField("progresso:",
                        $"Decorrido: {elapsed}\n" +
                        $"N de mensagens: {NumberOfMessages}\n" +
                        $"Ciclos realizados: {BatchNumber}\n" +
                        "Atual: ...");
                break;
            
            case BackupStage.InProgress:
                embedBuilder
                    .WithTitle("Em progresso...")
                    .WithColor(Color.Gold)
                    .AddField("Progresso:",
                        $"Decorrido: {elapsed}\n" +
                        $"N de mensagens: {NumberOfMessages}\n" +
                        $"Ciclos realizados: {BatchNumber}\n" +
                        $"Atual: {LastMessage.Author} {LastMessage.Timestamp.DateTime.ToShortDateString()} {LastMessage.Timestamp.DateTime.ToShortTimeString()}" +
                        $"\n{LastMessage.Content}");
                break;

            case BackupStage.Finished:
                embedBuilder
                    .WithTitle("Backup finalizado")
                    .WithColor(Color.Green)
                    .AddField("Estatisticas:",
                        $"Tempo decorrido: {elapsed}\n" +
                        $"N de mensagens: {NumberOfMessages}\n" +
                        $"Ciclos realizados: {BatchNumber}");
                break;

            case BackupStage.Failed:
                embedBuilder
                    .WithTitle("Falhou") //TODO rework failed response
                    .WithColor(Color.Red)
                    .AddField("Estatisticas:",
                        $"Tempo decorrido: {elapsed}\n" +
                        $"N de mensagens: {NumberOfMessages}\n" +
                        $"Ciclos realizados: {BatchNumber}");
                break;
        }

        return embedBuilder.Build();
    }

    private EmbedBuilder ConstructEmbed() //TODO All this ternary operation really the best way?
    {
        if (Author is null) throw new InvalidOperationException("Author property was not set");
        var startTime = StartTime.HasValue
            ? StartTime.Value.ToLongTimeString()
            : throw new InvalidOperationException("StartTime property was not set");
        var endTime = EndTime.HasValue
            ? EndTime.Value.ToLongTimeString()
            : "...";
        var firstMessage = StartMessage is not null
            ? $"{StartMessage.Author.Username} {StartMessage.Timestamp.DateTime.ToShortDateString()} {StartMessage.Timestamp.DateTime.ToShortTimeString()}" +
              $"\n{StartMessage.Content}"
            : "...";
        var lastMessage = LastMessage is not null
            ? $"{LastMessage.Author.Username} {LastMessage.Timestamp.DateTime.ToShortDateString()} {LastMessage.Timestamp.DateTime.ToShortTimeString()}" +
              $"\n{LastMessage.Content}"
            : "...";
        
        var firstMessageFieldEmbed = new EmbedFieldBuilder()
            .WithName("De:")
            .WithValue(firstMessage)
            .WithIsInline(false);
        var lastMessageFieldEmbed = new EmbedFieldBuilder()
            .WithName("Até:")
            .WithValue(lastMessage)
            .WithIsInline(false);
        
        var startTimeEmbed = new EmbedFieldBuilder()
            .WithName("Iniciado:")
            .WithValue(startTime)
            .WithIsInline(true);
        var endTimeEmbed = new EmbedFieldBuilder()
            .WithName("Terminado:")
            .WithValue(endTime)
            .WithIsInline(true);

        var madeByEmbed = new EmbedFooterBuilder()
            .WithText($"por: {Author.Username}")
            .WithIconUrl(Author.GetAvatarUrl());

        var embedBuilder = new EmbedBuilder()
            .WithTitle("Backup realizado!")
            .WithColor(Color.Green)
            .AddField(firstMessageFieldEmbed)
            .AddField(lastMessageFieldEmbed)
            .AddField(startTimeEmbed)
            .AddField(endTimeEmbed)
            .WithFooter(madeByEmbed);

        return embedBuilder;
    }
}

public enum BackupStage //TODO Should this really be here?
{
    Started,
    InProgress,
    Finished,
    Failed
}