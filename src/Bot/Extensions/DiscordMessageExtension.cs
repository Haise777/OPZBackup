// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord;

namespace OPZBot.Extensions;

public static class DiscordMessageExtension
{
    public static DateTime TimestampWithFixedTimezone(this IMessage message) 
        => message.Timestamp.DateTime.AddHours(Program.TimezoneAdjust);
}