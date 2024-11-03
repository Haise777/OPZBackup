// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using OPZBot.Utilities;

namespace OPZBot.Extensions;

public static class AsyncEventHandlerExtension
{
    public static Task InvokeAsync<TArgs>(this AsyncEventHandler<TArgs>? eventAsync, object? sender, TArgs e)
    {
        return eventAsync is null
            ? Task.CompletedTask
            : Task.WhenAll(eventAsync.GetInvocationList()
                .Cast<AsyncEventHandler<TArgs>>()
                .Select(f => f(sender, e)));
    }
}