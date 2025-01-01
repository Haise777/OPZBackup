using Discord;
using OPZBackup.Data.Models;
using OPZBackup.Extensions;
using OPZBackup.Services.Backup;
using OPZBackup.Services.Utils;

namespace OPZBackup.ResponseHandlers.Backup;

public class EmbedResponseFactory
{
    public Embed StartMessageEmbed(BackupContext context)
    {
        var parsedValues = ParseValuesToStrings(context);

        var embedBuilder = CreateNewEmbed(parsedValues);
        AddStartedStage(embedBuilder);
        AddProgressField(embedBuilder, context);
        return embedBuilder.Build();
    }

    public Embed BatchFinishedEmbed(BackupContext context, IMessage startMessage, IMessage lastMessage)
    {
        var parsedValues = ParseValuesToStrings(context, startMessage, lastMessage);

        var embedBuilder = CreateNewEmbed(parsedValues);
        AddInProgressStage(embedBuilder);
        AddProgressField(embedBuilder, context);
        return embedBuilder.Build();
    }

    public Embed CompletedEmbed(BackupContext context, IMessage startMessage, IMessage lastMessage, Channel channel)
    {
        var parsedValues = ParseValuesToStrings(context, startMessage, lastMessage, DateTime.Now);

        var embedBuilder = CreateNewEmbed(parsedValues);
        AddFinishedStage(embedBuilder);
        AddFinishedField(embedBuilder, context, channel);
        return embedBuilder.Build();
    }

    public Embed FailedEmbed(BackupContext context, Exception e)
    {
        var parsedValues = ParseValuesToStrings(context);

        var embedBuilder = CreateNewEmbed(parsedValues);
        AddFailedStage(embedBuilder);
        AddProgressField(embedBuilder, context);
        AddErrorField(embedBuilder, e);
        return embedBuilder.Build();
    }

    private void AddErrorField(EmbedBuilder builder, Exception e)
    {
        var type = e.GetType().Name;
        var message = $"\n```js\n" +
                      $"{e.GetType().Name} \n" +
                      $"'{e.Message}'```";

        builder.AddField("Assinatura do erro: ", message);
    }

    private string GetElapsedTime(DateTime startTime)
    {
        var elapsedTime = DateTime.Now - startTime;
        return $"{elapsedTime:hh\\:mm\\:ss}";
    }

    private void AddFailedStage(EmbedBuilder builder)
    {
        builder
            .WithTitle("Falhou")
            .WithColor(Color.Red);
    }

    private void AddFinishedStage(EmbedBuilder builder)
    {
        builder
            .WithTitle("Backup finalizado")
            .WithColor(Color.Green);
    }

    private void AddInProgressStage(EmbedBuilder builder)
    {
        builder
            .WithTitle("Em progresso...")
            .WithColor(Color.Gold);
    }

    private void AddStartedStage(EmbedBuilder builder)
    {
        builder
            .WithTitle("Em progresso...")
            .WithColor(Color.Gold);
    }

    private void AddProgressField(EmbedBuilder builder, BackupContext context, IMessage? currentMessage = null)
    {
        var totalFileSize = context.StatisticTracker.GetTotalStatistics().ByteSize;

        var progressString =
            $"Decorrido: {GetElapsedTime(context.BackupRegistry.Date)}\n" +
            $"N de mensagens: {context.MessageCount}\n" +
            $"N de arquivos: {context.FileCount} [{totalFileSize.ToFormattedString()}]\n" +
            $"Ciclos realizados: {context.BatchNumber} [{context.AverageBatchTime.Formatted()}]\n";

        if (currentMessage != null)
        {
            progressString +=
                $"Atual: {currentMessage.Author.Username} {currentMessage.TimestampWithFixedTimezone().ToShortDateString()} {currentMessage.Timestamp.DateTime.ToShortTimeString()}" +
                $"\n{currentMessage.Content}";
        }
        else
        {
            progressString += "Atual: ...";
        }

        builder.AddField("progresso:", progressString);
    }

    private void AddFinishedField(EmbedBuilder builder, BackupContext context, Channel channel)
    {
        var totalStatistic = context.StatisticTracker.GetTotalStatistics();

        var progressString =
            $"Decorrido: {GetElapsedTime(context.BackupRegistry.Date)}\n" +
            $"N de mensagens: {channel.MessageCount} [+{totalStatistic.MessageCount}]\n" +
            $"N de arquivos: {channel.FileCount} [+{totalStatistic.FileCount}]\n" +
            $"T. Armazenados: {channel.ByteSize.ToFormattedString()} [+{totalStatistic.ByteSize.ToFormattedString()}]\n" +
            $"T. Comprimidos: {channel.CompressedByteSize.ToFormattedString()}\n" +
            $"Ciclos realizados: {context.BatchNumber}\n";

        builder.AddField("Estatisticas:", progressString);
    }

    private ParsedValues ParseValuesToStrings(BackupContext context, IMessage? startMessage = null,
        IMessage? lastMessage = null, DateTime? endTime = null)
    {
        var startTime = context.BackupRegistry.Date;

        var parsedStartMessage = startMessage is not null
            ? $"{startMessage.Author.Username} {startMessage.TimestampWithFixedTimezone().ToShortDateString()} {startMessage.TimestampWithFixedTimezone().ToShortTimeString()}" +
              $"\n{startMessage.Content}"
            : "...";
        var parsedLastMessage = lastMessage is not null
            ? $"{lastMessage.Author.Username} {lastMessage.TimestampWithFixedTimezone().ToShortDateString()} {lastMessage.TimestampWithFixedTimezone().ToShortTimeString()}" +
              $"\n{lastMessage.Content}"
            : "...";
        var parsedStartTime = startTime.ToLongTimeString();

        var parsedEndTime = endTime.HasValue
            ? endTime.Value.ToLongTimeString()
            : "...";

        return new ParsedValues(parsedStartMessage, parsedLastMessage, parsedStartTime, parsedEndTime);
    }

    private record ParsedValues(string StartMessage, string LastMessage, string StartTime, string EndTime);

    private EmbedBuilder CreateNewEmbed(ParsedValues parsedValues)
    {
        var values = parsedValues;

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

        return embedBuilder;
    }
}