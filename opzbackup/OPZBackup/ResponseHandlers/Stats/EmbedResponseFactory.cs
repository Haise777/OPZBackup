using System.Text;
using Discord;
using OPZBackup.Data.Dto;
using OPZBackup.Data.Models;
using OPZBackup.Services.Utils;

namespace OPZBackup.ResponseHandlers.Stats;

public class EmbedResponseFactory
{
    // private EmbedBuilder CreateNewEmbed()
    // {
    //     var values = parsedValues;

    //     var firstMessageFieldEmbed = new EmbedFieldBuilder()
    //         .WithName("De:")
    //         .WithValue(values.StartMessage)
    //         .WithIsInline(false);
    //     var lastMessageFieldEmbed = new EmbedFieldBuilder()
    //         .WithName("Até:")
    //         .WithValue(values.LastMessage)
    //         .WithIsInline(false);

    //     var startTimeEmbed = new EmbedFieldBuilder()
    //         .WithName("Iniciado:")
    //         .WithValue(values.StartTime)
    //         .WithIsInline(true);
    //     var endTimeEmbed = new EmbedFieldBuilder()
    //         .WithName("Terminado:")
    //         .WithValue(values.EndTime)
    //         .WithIsInline(true);

    //     var embedBuilder = new EmbedBuilder()
    //         .AddField(firstMessageFieldEmbed)
    //         .AddField(lastMessageFieldEmbed)
    //         .AddField(startTimeEmbed)
    //         .AddField(endTimeEmbed);

    //     return embedBuilder;
    // }

    public Embed CreateChannelsStats(IEnumerable<Channel> channels)
    {
        var embedBuilder = CreateBaseEmbedBuilder("Status de todos os canais");

        int totalMessageCount = 0;
        int totalFileCount = 0;
        ulong compressedByteSize = 0;

        foreach (var channel in channels)
        {
            totalMessageCount += channel.MessageCount;
            totalFileCount += channel.FileCount;
            compressedByteSize += channel.CompressedByteSize;
        }

        // To add the totals here in a Embed field before the other fields are added
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"### Total do servidor em Backup");
        stringBuilder.AppendLine($"Mensagens totais: {totalMessageCount}");
        stringBuilder.AppendLine($"Arquivos totais: {totalFileCount}");
        stringBuilder.AppendLine($"Tamanho total: {compressedByteSize}");

        embedBuilder.WithDescription(stringBuilder.ToString());

        var channelTable = BuildChannelTableString(channels);

        embedBuilder.AddField("Lista de canais:",
            $"```\n{channelTable}\n```");

        return embedBuilder.Build();
    }

    public Embed CreateDetailedChannelStats(ChannelStats channelStats)
    {
        var embedBuilder = CreateBaseEmbedBuilder("Title");

        // To add the totals here in a Embed field before the other fields are added
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"### channel-name");
        stringBuilder.AppendLine($"Mensagens totais: {channelStats.numberOfMessages}");
        stringBuilder.AppendLine($"Arquivos totais: {channelStats.numberOfFiles}");
        stringBuilder.AppendLine($"Tamanho total: {channelStats.bytesize}");

        embedBuilder.WithDescription(stringBuilder.ToString());


        var userTable = BuildUserTableString(channelStats.users);

        embedBuilder.AddField("Lista de canais:",
            $"```\n{userTable}\n```");

        return embedBuilder.Build();
    }

    public Embed CreateUsersStats(IEnumerable<User> users)
    {
        var embedBuilder = CreateBaseEmbedBuilder();
        var userRows = new List<EmbedFieldBuilder>();

        foreach (var user in users)
        {
            userRows.Add(AddUserStatField(embedBuilder, user));
        }

        foreach (var row in userRows)
            embedBuilder.AddField(row);

        return embedBuilder.Build();
    }

    public Embed CreateDetailedUserStats(UserStats userStats)
    {



        throw new NotImplementedException();
    }

    private string GetChannelStatField(Channel channel)
    {
        return "";
    }

    private string BuildChannelTableString(IEnumerable<Channel> channels)
    {
        var builder = new StringBuilder();

        builder.AppendLine("╔════════════════╦═══════════╦══════════╦═════════╗");
        builder.AppendLine("║ Canal          ║ Mensagens ║ Arquivos ║ Tamanho ║");

        foreach (var channel in channels)
        {
            builder.AppendLine("╠════════════════╬═══════════╬══════════╬═════════╣");
            builder.AppendLine(
                $"║ {FitText(channel.Name, 14)} ║ {FitText(channel.MessageCount.ToString(), 9)} ║ {FitText(channel.FileCount.ToString(), 8)} ║ {FitText(channel.CompressedByteSize.ToFormattedString(), 8)} ║");
        }

        builder.Append("╚════════════════╩═══════════╩══════════╩═════════╝");
        return builder.ToString();
    }

        private string BuildUserTableString(IEnumerable<User> users)
    {
        var builder = new StringBuilder();

        builder.AppendLine("╔════════════════╦═══════════╦══════════╦═════════╗");
        builder.AppendLine("║ Usuário        ║ Mensagens ║ Arquivos ║ Tamanho ║");

        foreach (var user in users)
        {
            builder.AppendLine("╠════════════════╬═══════════╬══════════╬═════════╣");
            builder.AppendLine(
                $"║ {FitText(user.Username, 14)} ║ {FitText(user.MessageCount.ToString(), 9)} ║ {FitText(user.FileCount.ToString(), 8)} ║ {FitText(user.ByteSize.ToFormattedString(), 8)} ║");
        }

        builder.Append("╚════════════════╩═══════════╩══════════╩═════════╝");
        return builder.ToString();
    }

    string FitText(string text, int width)
    {
        // If the text fits within the width, pad it on the right.
        if (text.Length <= width)
        {
            return text.PadRight(width);
        }
        else
        {
            // If the width is too small to even display ellipsis, just take a substring.
            if (width <= 3)
                return text.Substring(0, width);
            // Otherwise, reserve space for "..." at the end.
            return text.Substring(0, width - 3) + "...";
        }
    }


    private EmbedBuilder CreateBaseEmbedBuilder(string title)
    {
        var embedBuilder = new EmbedBuilder();

        // Base stuff for the embed here
        embedBuilder.WithTitle(title);
        embedBuilder.WithColor(Color.Blue);

        return embedBuilder;
    }
}