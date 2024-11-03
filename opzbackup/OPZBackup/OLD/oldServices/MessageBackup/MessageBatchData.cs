// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using OPZBot.Data.Models;

namespace OPZBot.Services.MessageBackup;

public record MessageBatchData(IEnumerable<User> Users, IEnumerable<Message> Messages, int FileCount);