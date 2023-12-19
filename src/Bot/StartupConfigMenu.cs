// Copyright (c) 2023, Gabriel Shimabucoro
// All rights reserved.
// 
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

namespace OPZBot;
internal class StartupConfigMenu : BotConfigService
{
    public void Initialize()
    {
        Menu();
        WriteConfigFile(Config);
        Console.Clear();
    }

    private void Menu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine(
                $"OPZBot - ver{Program.APP_VER} \n" +
                $"\n" +
                $"[R] Run \n" +
                $"[C] Config \n" +
                $"[X] Exit\n");

            switch (Console.ReadKey().KeyChar)
            {
                case 'R':
                case 'r':
                    return;
                case 'C':
                case 'c':
                    ConfigMenu();
                    break;
                case 'X':
                case 'x':
                    WriteConfigFile(Config);
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine(" is not a valid input");
                    Console.ReadKey();
                    break;
            }
        }
    }

    private void ConfigMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine(
                $"OPZBot - ver{Program.APP_VER} \n" +
                $"Set bot startup values \n" +
                $"\n" +
                $"[B] Bot token > {Config.Token?[^7..]?.Insert(0, "xxxxxx...")}\n" +
                $"[A] Main admin role id > {Config.MainAdminRoleId}\n" +
                $"[S] Database connection string > {Config.ConnectionString}\n" +
                $"[T] Timezone adjust value > {Config.TimezoneAdjust}\n" +
                $"[C] General cooldowns > {Config.RunWithCooldowns}\n" +
                $"[X] Return\n");

#if DEBUG
            Console.WriteLine($"[D] DEBUG: Test guild id > {Config.TestGuildId}\n");
#endif
            var input = "";
            switch (Console.ReadKey().KeyChar)
            {
                case 'B':
                case 'b':
                    input = WriteInput("Bot token");
                    if (ConfirmChanges(Config.Token, input, true))
                        Config.Token = input;
                    break;
                case 'A':
                case 'a':
                    input = WriteInput("Main admin role id");
                    if (ConfirmChanges(Config.MainAdminRoleId.ToString(), input))
                        Config.MainAdminRoleId =
                            ulong.TryParse(input, out var adminRoleId) ? adminRoleId : null;
                    break;
                case 'S':
                case 's':
                    input = WriteInput("Database connection string");
                    if (ConfirmChanges(Config.ConnectionString, input))
                        Config.ConnectionString = input;
                    break;
                case 'T':
                case 't':
                    input = WriteInput("Timezone adjust value");
                    Config.TimezoneAdjust = int.TryParse(input, out var timezoneAdjust) ? timezoneAdjust : null;

                    break;
                case 'C':
                case 'c':
                    Config.RunWithCooldowns = !Config.RunWithCooldowns;

                    break;
                case 'X':
                case 'x':
                    return;
#if DEBUG
                case 'D':
                case 'd':
                    input = WriteInput("DEBUG: Test guild id");
                    if (ConfirmChanges(Config.TestGuildId.ToString(), input))
                        Config.TestGuildId = ulong.TryParse(input, out var testGuildId) ? testGuildId : null;
                    break;
#endif
                default:
                    Console.WriteLine(" is not a valid input");
                    Console.ReadKey();
                    break;
            }
        }
    }

    private string? WriteInput(string option)
    {
        Console.Clear();
        Console.Write($"{option} > ");
        return Console.ReadLine();
    }

    private bool ConfirmChanges(string? oldValues, string? newValues, bool shouldCensor = false)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine(
                "Confirm changes\n" +
                $"old: {(shouldCensor ? oldValues?[^7..]?.Insert(0, "xxxxxx...") : oldValues)}\n" +
                $"new: {newValues}");

            Console.WriteLine("\n [y] / [n]");

            switch (Console.ReadKey().KeyChar)
            {
                case 'Y':
                case 'y':
                    return true;
                case 'N':
                case 'n':
                    return false;
                default:
                    Console.Write(" is not a valid choice");
                    Console.ReadKey();
                    continue;
            }
        }
    }
}