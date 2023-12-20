// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

namespace OPZBot.Extensions;

public static class EnumerableExtension
{
    public static IEnumerable<TSource> ExcludeFirst<TSource>(this IEnumerable<TSource> source) where TSource : class
    {
        return source.Where(x => x != source.First());
    }
}