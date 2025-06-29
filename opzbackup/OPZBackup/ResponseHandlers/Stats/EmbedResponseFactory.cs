using System.Text;
using Discord;
using OPZBackup.Data.Dto;
using OPZBackup.Data.Models;
using OPZBackup.Services.Stats;
using OPZBackup.Services.Utils;

namespace OPZBackup.ResponseHandlers.Stats;

public class EmbedResponseFactory
{
    //TODO: Heavily refactor this to reduce code duplication across all methods
    public Embed CreateChannelsStats(Channel[][] channels, int currentPage = 0)
    {
        var embedBuilder = CreateBaseEmbedBuilder("Status de todos os canais");

        int totalMessageCount = 0;
        int totalFileCount = 0;
        ulong compressedByteSize = 0;

        foreach (var channelChunk in channels)
        {
            foreach (var channel in channelChunk)
            {
                totalMessageCount += channel.MessageCount;
                totalFileCount += channel.FileCount;
                compressedByteSize += channel.CompressedByteSize;
            }
        }

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"### Total do servidor em Backup");
        stringBuilder.AppendLine($"Mensagens totais: {totalMessageCount}");
        stringBuilder.AppendLine($"Arquivos totais: {totalFileCount}");
        stringBuilder.AppendLine($"Tamanho total: {compressedByteSize.ToFormattedString()}");

        embedBuilder.WithDescription(stringBuilder.ToString());

        var channelTable = BuildChannelTableString(channels[currentPage]);

        embedBuilder.AddField("Lista de canais:",
            $"```\n{channelTable}\n" +
            $"<{currentPage + 1}/{channels.Length}>```");

        return embedBuilder.Build();
    }

    public Embed CreateDetailedChannelStats(ChannelStats channelStats, int currentPage = 0)
    {
        var embedBuilder = CreateBaseEmbedBuilder("Title");

        // To add the totals here in a Embed field before the other fields are added
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"### channel-name");
        stringBuilder.AppendLine($"Mensagens totais: {channelStats.numberOfMessages}");
        stringBuilder.AppendLine($"Arquivos totais: {channelStats.numberOfFiles}");
        stringBuilder.AppendLine($"Tamanho total: {channelStats.bytesize.ToFormattedString()}");

        embedBuilder.WithDescription(stringBuilder.ToString());

        var userTable = BuildUserTableString(channelStats.users[currentPage]);

        embedBuilder.AddField("Usuários presentes:",
            $"```\n{userTable}\n" +
            $"<{currentPage + 1}/{channelStats.users.Length}>```");

        return embedBuilder.Build();
    }

    public Embed CreateUsersStats(User[][] users, int currentPage = 0)
    {
        var embedBuilder = CreateBaseEmbedBuilder("Todos os usuários");

        var userTable = BuildUserTableString(users[currentPage]);

        embedBuilder.AddField("Usuários:",
            $"```\n{userTable}\n" +
            $"<{currentPage + 1}/{users.Length}>```");

        return embedBuilder.Build();
    }

    public Embed CreateDetailedUserStats(UserStatsWithUsernames userStats, string selectedTable, int currentPage = 0)
    {
        var embedBuilder = CreateBaseEmbedBuilder("titulo");
        var user = userStats.user ?? throw new NullReferenceException();

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"## {userStats.user.Username}");
        stringBuilder.AppendLine($"Mensagens totais: {user.MessageCount}");
        stringBuilder.AppendLine($"Arquivos totais: {user.FileCount}");
        stringBuilder.AppendLine($"Tamanho total: {user.ByteSize.ToFormattedString()}");

        embedBuilder.WithDescription(stringBuilder.ToString());

        if (selectedTable == "table1")
        {
            var topWords = BuildTopWordsTableString(userStats.MostCommonWords[currentPage].ToDictionary());
            embedBuilder.AddField("Top palavras:",
                $"```\n{topWords}\n" +
                $"<{currentPage + 1}/{userStats.MostCommonWords.Length}>```");
        }
        else if (selectedTable == "table2")
        {
            var mentionTable = BuildMentionTableString(userStats.NumberOfMentions[currentPage].ToDictionary());
            embedBuilder.AddField("Top menções:",
                $"```\n{mentionTable}\n" +
                $"<{currentPage + 1}/{userStats.NumberOfMentions.Length}>```");
        }
        else if (selectedTable == "table3")
        {
            var fileTypeTable = BuildFileTypeTableString(userStats.fileTypeStats);
            embedBuilder.AddField("Anexos enviados:",
                $"```\n{fileTypeTable}\n```");
        }

        return embedBuilder.Build();
    }

    private string BuildTopWordsTableString(Dictionary<string, int> mostCommonWords)
    {
        var builder = new StringBuilder();

        builder.AppendLine("╔════════════════╦═════════╗");
        builder.AppendLine("║ Palavra        ║ Quanti. ║");

        foreach (var word in mostCommonWords)
        {
            builder.AppendLine("╠════════════════╬═════════╣");
            builder.AppendLine(
                $"║ {FitText(word.Key, 14)} ║ {FitText(word.Value.ToString(), 7)} ║");
        }

        builder.Append("╚════════════════╩═════════╝");
        return builder.ToString();
    }

    private string BuildFileTypeTableString(FileTypeStats fileTypeStats)
    {
        var builder = new StringBuilder();

        builder.AppendLine("╔════════════════╦═════════╗");
        builder.AppendLine("║ Tipo           ║ Quanti. ║");

        var dictionary = new Dictionary<string, int>()
        {
            { "Imagens", fileTypeStats.ImageCount },
            { "Videos", fileTypeStats.VideoCount },
            { "Audios", fileTypeStats.AudioCount },
            { "Outros", fileTypeStats.OtherCount },
        };

        foreach (var fileType in dictionary)
        {
            builder.AppendLine("╠════════════════╬═════════╣");
            builder.AppendLine(
                $"║ {FitText(fileType.Key, 14)} ║ {FitText(fileType.Value.ToString(), 7)} ║");
        }

        builder.Append("╚════════════════╩═════════╝");
        return builder.ToString();
    }

    private string BuildMentionTableString(Dictionary<string, int> mentions)
    {
        var builder = new StringBuilder();

        builder.AppendLine("╔════════════════╦═════════╗");
        builder.AppendLine("║ Usuário        ║ Menções ║");

        foreach (var mention in mentions)
        {
            builder.AppendLine("╠════════════════╬═════════╣");
            builder.AppendLine(
                $"║ {FitText(mention.Key, 14)} ║ {FitText(mention.Value.ToString(), 7)} ║");
        }

        builder.Append("╚════════════════╩═════════╝");
        return builder.ToString();
    }

    private string BuildChannelTableString(IEnumerable<Channel> channels)
    {
        var builder = new StringBuilder();

        builder.AppendLine("╔═════════════════╦═══════════╦══════════╦════════════╗");
        builder.AppendLine("║ Canal           ║ Mensagens ║ Arquivos ║ Tamanho    ║");

        foreach (var channel in channels)
        {
            builder.AppendLine("╠═════════════════╬═══════════╬══════════╬════════════╣");
            builder.AppendLine(
                $"║ {FitText(channel.Name, 15)} ║ {FitText(channel.MessageCount.ToString(), 9)} ║ {FitText(channel.FileCount.ToString(), 8)} ║ {FitText(channel.CompressedByteSize.ToFormattedString(), 10)} ║");
        }

        builder.Append("╚═════════════════╩═══════════╩══════════╩════════════╝");
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
        var filteredText = new string(text
            .Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || char.IsPunctuation(c)).ToArray());
        if (filteredText.Length <= width)
        {
            return filteredText.PadRight(width);
        }
        else
        {
            if (width <= 3)
                return filteredText.Substring(0, width);

            return filteredText.Substring(0, width - 3) + "...";
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