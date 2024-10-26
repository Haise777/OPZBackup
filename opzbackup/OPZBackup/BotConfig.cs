// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

namespace OPZBackup;

internal class BotConfig
{
    public string? Token { get; set; }
    public ulong? MainAdminRoleId { get; set; }
    public bool RunWithCooldowns { get; set; } = true;
    public ulong? TestGuildId { get; set; }
    public int? TimezoneAdjust { get; set; }
}