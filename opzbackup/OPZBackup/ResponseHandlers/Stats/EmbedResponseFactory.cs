using System.Text;
using Discord;
using OPZBackup.Data.Dto;
using OPZBackup.Data.Models;
using OPZBackup.Services.Utils;

namespace OPZBackup.ResponseHandlers.Stats;

public class EmbedResponseFactory
{
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

        embedBuilder.AddField("Usuários presentes:",
            $"```\n{userTable}\n```");

        return embedBuilder.Build();
    }

    public Embed CreateUsersStats(IEnumerable<User> users)
    {
        var embedBuilder = CreateBaseEmbedBuilder("Todos os usuários");

        var userTable = BuildUserTableString(users);

        embedBuilder.AddField("Usuários:",
            $"```\n{userTable}\n```");

        return embedBuilder.Build();
    }

    public Embed CreateDetailedUserStats(UserStats userStats)
    {
        var embedBuilder = CreateBaseEmbedBuilder("titulo");
        var user = userStats.user ?? throw new NullReferenceException();

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"### user-name");
        stringBuilder.AppendLine($"Mensagens totais: {user.MessageCount}");
        stringBuilder.AppendLine($"Arquivos totais: {user.FileCount}");
        stringBuilder.AppendLine($"Tamanho total: {user.ByteSize}");

        embedBuilder.WithDescription(stringBuilder.ToString());

        embedBuilder.AddField("Top Palavras",
            GetTopWords(userStats.MostCommonWords));

        var mentionTable = BuildMentionTableString(userStats.NumberOfMentions);
        embedBuilder.AddField("Top menções:",
            $"```\n{mentionTable}\n```");

        return embedBuilder.Build();
    }

    private string GetTopWords(Dictionary<string, int> topWords)
    {
        var builder = new StringBuilder();

        foreach (var topWord in topWords)
        {
            builder.AppendLine($"* {topWord.Key}");
        }

        return builder.ToString();
    }

    private string BuildMentionTableString(Dictionary<ulong, int> mentions)
    {
        var builder = new StringBuilder();

        builder.AppendLine("╔════════════════╦═════════╗");
        builder.AppendLine("║ Usuário        ║ Menções ║");

        foreach (var mention in mentions)
        {
            builder.AppendLine("╠════════════════╬═════════╣");
            builder.AppendLine(
                $"║ {FitText(mention.Key.ToString(), 14)} ║ {FitText(mention.Value.ToString(), 7)} ║");
        }

        builder.Append("╚════════════════╩═════════╝");
        return builder.ToString();
    }

    private string BuildChannelTableString(IEnumerable<Channel> channels)
    {
        var builder = new StringBuilder();

        builder.AppendLine("╔════════════════╦═══════════╦══════════╦════════════╗");
        builder.AppendLine("║ Canal          ║ Mensagens ║ Arquivos ║ Tamanho    ║");

        foreach (var channel in channels)
        {
            builder.AppendLine("╠════════════════╬═══════════╬══════════╬════════════╣");
            builder.AppendLine(
                $"║ {FitText(channel.Name, 14)} ║ {FitText(channel.MessageCount.ToString(), 9)} ║ {FitText(channel.FileCount.ToString(), 8)} ║ {FitText(channel.CompressedByteSize.ToFormattedString(), 10)} ║");
        }

        builder.Append("╚════════════════╩═══════════╩══════════╩════════════╝");
        return builder.ToString();
    }

        private string BuildUserTableString(IEnumerable<User> users)
    {
        var builder = new StringBuilder();

        builder.AppendLine("╔════════════════╦═══════════╦══════════╦════════════╗");
        builder.AppendLine("║ Usuário        ║ Mensagens ║ Arquivos ║ Tamanho    ║");

        foreach (var user in users)
        {
            builder.AppendLine("╠════════════════╬═══════════╬══════════╬════════════╣");
            builder.AppendLine(
                $"║ {FitText(user.Username, 14)} ║ {FitText(user.MessageCount.ToString(), 9)} ║ {FitText(user.FileCount.ToString(), 8)} ║ {FitText(user.ByteSize.ToFormattedString(), 10)} ║");
        }

        builder.Append("╚════════════════╩═══════════╩══════════╩════════════╝");
        return builder.ToString();
    }

    string FitText(string text, int width)
    {
        if (text.Length <= width)
        {
            return text.PadRight(width);
        }
        else
        {
            if (width <= 3)
                return text.Substring(0, width);

            return text.Substring(0, width - 3) + "...";
        }
    }


    private EmbedBuilder CreateBaseEmbedBuilder(string title)
    {
        var embedBuilder = new EmbedBuilder();

        embedBuilder.WithTitle(title);
        embedBuilder.WithColor(Color.Blue);

        return embedBuilder;
    }
}