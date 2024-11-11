﻿using Discord;
using OPZBackup.Extensions;
//Aliases for named tuples
using ParsedValues = (string StartMessage, string LastMessage, string StartTime, string EndTime);

namespace OPZBackup.ResponseHandlers.Backup;

public class ResponseBuilder
{
    private EmbedBuilder? _embedBuilder;
    private DateTime? _endTime;
    public DateTime? StartTime { get; set; }
    public IMessage? StartMessage { get; set; }
    public IMessage? CurrentMessage { get; set; }
    public IMessage? LastMessage { get; set; }
    public IUser? Author { get; private set; }
    public int BatchNumber { get; private set; }
    public int NumberOfMessages { get; private set; }
    public int NumberOfFiles { get; private set; }
    public TimeSpan ElapsedTime { get; private set; }

    private string ElapsedTimeString => $"{ElapsedTime:hh\\:mm\\:ss}";

    public ResponseBuilder SetStartTime(DateTime startTime)
    {
        StartTime = startTime;
        return this;
    }

    public ResponseBuilder SetStartMessage(IMessage message)
    {
        StartMessage = message;
        return this;
    }

    public ResponseBuilder SetAuthor(IUser author)
    {
        Author = author;
        return this;
    }

    public ResponseBuilder UpdateElapsedTime()
    {
        ElapsedTime = StartTime.HasValue ? (DateTime.Now - StartTime).Value : TimeSpan.Zero;
        return this;
    }

    public Embed Build(ProgressStage stage)
    {
        switch (stage)
        {
            case ProgressStage.Started:
                AddStartedStage();
                break;
            case ProgressStage.Finished:
                AddFinishedStage();
                break;
            case ProgressStage.Failed:
                AddFailedStage();
                break;
            case ProgressStage.InProgress:
                AddInProgressStage();
                break;
        }

        return _embedBuilder!.Build();
    }

    private void AddFailedStage()
    {
        _embedBuilder = CreateNewEmbed()
            .WithTitle("Falhou")
            .WithColor(Color.Red)
            .AddField("Estatisticas:", GenerateProgressField(false));
    }

    private void AddFinishedStage()
    {
        _endTime = DateTime.Now;
        _embedBuilder = CreateNewEmbed()
            .WithTitle("Backup finalizado")
            .WithColor(Color.Green)
            .AddField("Estatisticas:", GenerateProgressField(false));
    }

    private void AddInProgressStage()
    {
        _embedBuilder = CreateNewEmbed()
            .WithTitle("Em progresso...")
            .WithColor(Color.Gold)
            .AddField("Progresso:", GenerateProgressField(true));
    }

    private void AddStartedStage()
    {
        _embedBuilder = CreateNewEmbed()
            .WithTitle("Em progresso...")
            .WithColor(Color.Gold)
            .AddField("progresso:", GenerateProgressField(true, "..."));
    }

    private string GenerateProgressField(bool withActual, string? actual = null)
    {
        var progressString =
            $"Decorrido: {ElapsedTimeString}\n" +
            $"N de mensagens: {NumberOfMessages}\n" +
            $"N de arquivos: {NumberOfFiles}\n" +
            $"Ciclos realizados: {BatchNumber}\n";

        if (withActual)
        {
            if (actual != null)
                progressString += $"Atual: {actual}";
            else
                progressString +=
                    $"Atual: {CurrentMessage.Author.Username} {CurrentMessage.TimestampWithFixedTimezone().ToShortDateString()} {CurrentMessage.Timestamp.DateTime.ToShortTimeString()}" +
                    $"\n{CurrentMessage.Content}";
        }

        return progressString;
    }

    private ParsedValues ParseValuesToStrings()
    {
        var parsedToString = new string[4];
        parsedToString[0] = StartMessage is not null
            ? $"{StartMessage.Author.Username} {StartMessage.TimestampWithFixedTimezone().ToShortDateString()} {StartMessage.TimestampWithFixedTimezone().ToShortTimeString()}" +
              $"\n{StartMessage.Content}"
            : "...";
        parsedToString[1] = LastMessage is not null
            ? $"{LastMessage.Author.Username} {LastMessage.TimestampWithFixedTimezone().ToShortDateString()} {LastMessage.TimestampWithFixedTimezone().ToShortTimeString()}" +
              $"\n{LastMessage.Content}"
            : "...";
        parsedToString[2] = StartTime.HasValue
            ? StartTime.Value.ToLongTimeString()
            : "...";
        parsedToString[3] = _endTime.HasValue
            ? _endTime.Value.ToLongTimeString()
            : "...";

        return new ParsedValues(parsedToString[0], parsedToString[1], parsedToString[2], parsedToString[3]);
    }

    private EmbedBuilder CreateNewEmbed()
    {
        var values = ParseValuesToStrings();

        var firstMessageFieldEmbed = new EmbedFieldBuilder()
            .WithName("De:")
            .WithValue(values.StartMessage)
            .WithIsInline(false);
        var lastMessageFieldEmbed = new EmbedFieldBuilder()
            .WithName("Até:")
            .WithValue(values.LastMessage)
            .WithIsInline(false);

        var startTimeEmbed = new EmbedFieldBuilder()
            .WithName("Iniciado:")
            .WithValue(values.StartTime)
            .WithIsInline(true);
        var endTimeEmbed = new EmbedFieldBuilder()
            .WithName("Terminado:")
            .WithValue(values.EndTime)
            .WithIsInline(true);

        var embedBuilder = new EmbedBuilder()
            .AddField(firstMessageFieldEmbed)
            .AddField(lastMessageFieldEmbed)
            .AddField(startTimeEmbed)
            .AddField(endTimeEmbed);

        if (Author is not null)
            embedBuilder.WithFooter(new EmbedFooterBuilder()
                .WithText($"por: {Author.Username}")
                .WithIconUrl(Author.GetAvatarUrl()));

        return embedBuilder;
    }

    public ResponseBuilder SetBatchNumber(int batchNumber)
    {
        BatchNumber = batchNumber;
        return this;
    }

    public ResponseBuilder SetMessageCount(int messageCount)
    {
        NumberOfMessages = messageCount;
        return this;
    }

    public ResponseBuilder SetFileCount(int fileCount)
    {
        NumberOfFiles = fileCount;
        return this;
    }

    public ResponseBuilder SetCurrentMessage(IMessage message)
    {
        CurrentMessage = message;
        return this;
    }

    public ResponseBuilder CurrentAsLastMessage()
    {
        LastMessage = CurrentMessage;
        return this;
    }
}

public enum ProgressStage
{
    Started,
    InProgress,
    Failed,
    Finished
}