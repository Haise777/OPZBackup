// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Discord.Interactions;

namespace OPZBackup.Services;

public interface IResponseHandler
{
    Task SendNotRightPermissionAsync(SocketInteractionContext context);
}