using Discord;
using OPZBackup.Data.Dto;
using OPZBackup.Data.Models;

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

    internal Embed CreateChannelsStats(IEnumerable<Channel> channels)
    {
        var embedBuilder = CreateBaseEmbedBuilder();
        var channelRows = new List<EmbedFieldBuilder>();
        int totalMessageCount = 0;
        int totalFileCount = 0;
        ulong compressedByteSize = 0;

        foreach (var channel in channels)
        {
            channelRows.Add(AddChannelStatField(embedBuilder, channel));
            totalMessageCount += channel.MessageCount;
            totalFileCount += channel.FileCount;
            compressedByteSize += channel.CompressedByteSize;
        }

        // To add the totals here in a Embed field before the other fields are added

        foreach (var row in channelRows)
            embedBuilder.AddField(row);

        return embedBuilder.Build();
    }

    internal Embed CreateDetailedChannelStats(ChannelStats channelStats)
    {




        throw new NotImplementedException();
    }

    internal Embed CreateUsersStats(IEnumerable<User> users)
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

    internal Embed CreateDetailedUserStats(UserStats userStats)
    {



        throw new NotImplementedException();
    }

    private EmbedFieldBuilder AddChannelStatField(EmbedBuilder embedBuilder, Channel channel)
    {
        return new EmbedFieldBuilder()
        .WithName("");
    }

    private EmbedFieldBuilder AddUserStatField(EmbedBuilder embedBuilder, User user)
    {
        return new EmbedFieldBuilder()
        .WithName("");
    }

    private EmbedBuilder CreateBaseEmbedBuilder()
    {
        var embedBuilder = new EmbedBuilder();

        // Base stuff for the embed here

        return embedBuilder;
    }
}