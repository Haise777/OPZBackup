// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord;
using OPZBackup.Extensions;
//Aliases for named tuples
using ParsedValues = (string StartMessage, string LastMessage, string StartTime, string EndTime);
using EmbedData = (int BatchNumber, int NumberOfMessages, int NumberOfFiles, string Elapsed);

namespace OPZBackup.Services.MessageBackup;

public class ResponseBuilder
{
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public IMessage? StartMessage { get; set; }
    public IMessage? CurrentMessage { get; set; }
    public IMessage? LastMessage { get; set; }
    public IUser? Author { get; set; }

    public Embed Build(int batchNumber, int numberOfMessages, int numberOfFiles, ProgressStage stage)
    {
        var embedBuilder = ConstructEmbed();
        var t = StartTime.HasValue ? (DateTime.Now - StartTime).Value : TimeSpan.Zero;
        var elapsed = $"{t:hh\\:mm\\:ss}"; //DO NOT use verbatim string, it causes error in the API
        var embedData = new EmbedData(batchNumber, numberOfMessages, numberOfFiles, elapsed);

        AddStage(stage, embedBuilder, embedData);
        return embedBuilder.Build();
    }

    private void AddStage(ProgressStage stage, EmbedBuilder embedBuilder, EmbedData embedData)
    {
        switch (stage)
        {
            case ProgressStage.Started:
                AddStartedStage(embedBuilder, embedData);
                break;

            case ProgressStage.InProgress:
                if (CurrentMessage is null)
                    throw new InvalidOperationException(
                        "'CurrentMessage' property is not optional when 'InProgress' is set on Builder");
                AddInProgressStage(embedBuilder, embedData);
                break;

            case ProgressStage.Finished:
                AddFinishedStage(embedBuilder, embedData);
                break;

            case ProgressStage.Failed:
                AddFailedStage(embedBuilder, embedData);
                break;
        }
    }

    private void AddFailedStage(EmbedBuilder embedBuilder, EmbedData data)
    {
        embedBuilder
            .WithTitle("Falhou")
            .WithColor(Color.Red)
            .AddField("Estatisticas:",
                $"Tempo decorrido: {data.Elapsed}\n" +
                $"N de mensagens: {data.NumberOfMessages}\n" +
                $"N de arquivos: {data.NumberOfFiles}\n" +
                $"Ciclos realizados: {data.BatchNumber}");
    }

    private void AddFinishedStage(EmbedBuilder embedBuilder, EmbedData data)
    {
        embedBuilder
            .WithTitle("Backup finalizado")
            .WithColor(Color.Green)
            .AddField("Estatisticas:",
                $"Tempo decorrido: {data.Elapsed}\n" +
                $"N de mensagens: {data.NumberOfMessages}\n" +
                $"N de arquivos: {data.NumberOfFiles}\n" +
                $"Ciclos realizados: {data.BatchNumber}");
    }

    private void AddInProgressStage(EmbedBuilder embedBuilder, EmbedData data)
    {
        embedBuilder
            .WithTitle("Em progresso...")
            .WithColor(Color.Gold)
            .AddField("Progresso:",
                $"Decorrido: {data.Elapsed}\n" +
                $"N de mensagens: {data.NumberOfMessages}\n" +
                $"N de arquivos: {data.NumberOfFiles}\n" +
                $"Ciclos realizados: {data.BatchNumber}\n" +
                $"Atual: {CurrentMessage.Author} {CurrentMessage.TimestampWithFixedTimezone().ToShortDateString()} {CurrentMessage.Timestamp.DateTime.ToShortTimeString()}" +
                $"\n{CurrentMessage.Content}");
    }

    private void AddStartedStage(EmbedBuilder embedBuilder, EmbedData data)
    {
        embedBuilder
            .WithTitle("Em progresso...")
            .WithColor(Color.Gold)
            .AddField("progresso:",
                $"Decorrido: {data.Elapsed}\n" +
                $"N de mensagens: {data.NumberOfMessages}\n" +
                $"N de arquivos: {data.NumberOfFiles}\n" +
                $"Ciclos realizados: {data.BatchNumber}\n" +
                "Atual: ...");
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
        parsedToString[3] = EndTime.HasValue
            ? EndTime.Value.ToLongTimeString()
            : "...";

        return new ParsedValues(parsedToString[0], parsedToString[1], parsedToString[2], parsedToString[3]);
    }

    private EmbedBuilder ConstructEmbed()
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
}

public enum ProgressStage
{
    Started,
    InProgress,
    Finished,
    Failed
}