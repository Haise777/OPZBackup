// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord;
using OPZBot.Extensions;

namespace OPZBot.Services.MessageBackup;

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
        var elapsed = $"{t.TotalHours:00}:{t:mm\\:ss}";

        switch (stage)
        {
            case ProgressStage.Started:
                embedBuilder
                    .WithTitle("Em progresso...")
                    .WithColor(Color.Gold)
                    .AddField("progresso:",
                        $"Decorrido: {elapsed}\n" +
                        $"N de mensagens: {numberOfMessages}\n" +
                        $"N de arquivos: {numberOfFiles}\n" +
                        $"Ciclos realizados: {batchNumber}\n" +
                        "Atual: ...");
                break;

            case ProgressStage.InProgress:
                if (CurrentMessage is null)
                    throw new InvalidOperationException(
                        "'CurrentMessage' property is not optional when 'InProgress' is set on Builder");
                embedBuilder
                    .WithTitle("Em progresso...")
                    .WithColor(Color.Gold)
                    .AddField("Progresso:",
                        $"Decorrido: {elapsed}\n" +
                        $"N de mensagens: {numberOfMessages}\n" +
                        $"N de arquivos: {numberOfFiles}\n" +
                        $"Ciclos realizados: {batchNumber}\n" +
                        $"Atual: {CurrentMessage.Author} {CurrentMessage.TimestampWithFixedTimezone().ToShortDateString()} {CurrentMessage.Timestamp.DateTime.ToShortTimeString()}" +
                        $"\n{CurrentMessage.Content}");
                break;

            case ProgressStage.Finished:
                embedBuilder
                    .WithTitle("Backup finalizado")
                    .WithColor(Color.Green)
                    .AddField("Estatisticas:",
                        $"Tempo decorrido: {elapsed}\n" +
                        $"N de mensagens: {numberOfMessages}\n" +
                        $"N de arquivos: {numberOfFiles}\n" +
                        $"Ciclos realizados: {batchNumber}");
                break;

            case ProgressStage.Failed:
                embedBuilder
                    .WithTitle("Falhou")
                    .WithColor(Color.Red)
                    .AddField("Estatisticas:",
                        $"Tempo decorrido: {elapsed}\n" +
                        $"N de mensagens: {numberOfMessages}\n" +
                        $"N de arquivos: {numberOfFiles}\n" +
                        $"Ciclos realizados: {batchNumber}");
                break;
        }

        return embedBuilder.Build();
    }

    private string[] ParseValuesToStrings()
    {
        var parsedValues = new string[4];
        parsedValues[0] = StartMessage is not null
            ? $"{StartMessage.Author.Username} {StartMessage.TimestampWithFixedTimezone().ToShortDateString()} {StartMessage.TimestampWithFixedTimezone().ToShortTimeString()}" +
              $"\n{StartMessage.Content}"
            : "...";
        parsedValues[1] = LastMessage is not null
            ? $"{LastMessage.Author.Username} {LastMessage.TimestampWithFixedTimezone().ToShortDateString()} {LastMessage.TimestampWithFixedTimezone().ToShortTimeString()}" +
              $"\n{LastMessage.Content}"
            : "...";
        parsedValues[2] = StartTime.HasValue
            ? StartTime.Value.ToLongTimeString()
            : "...";
        parsedValues[3] = EndTime.HasValue
            ? EndTime.Value.ToLongTimeString()
            : "...";

        return parsedValues;
    }

    private EmbedBuilder ConstructEmbed()
    {
        var values = ParseValuesToStrings();

        var firstMessageFieldEmbed = new EmbedFieldBuilder()
            .WithName("De:")
            .WithValue(values[0])
            .WithIsInline(false);
        var lastMessageFieldEmbed = new EmbedFieldBuilder()
            .WithName("Até:")
            .WithValue(values[1])
            .WithIsInline(false);

        var startTimeEmbed = new EmbedFieldBuilder()
            .WithName("Iniciado:")
            .WithValue(values[2])
            .WithIsInline(true);
        var endTimeEmbed = new EmbedFieldBuilder()
            .WithName("Terminado:")
            .WithValue(values[3])
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