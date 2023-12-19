using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace OPZBot;

internal class StartupConfig
{
    private readonly BotConfig _botConfig;

    public StartupConfig()
    {
        var startupConfig = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("config.json")
            .Build();

        _botConfig = new()
        {
            Token = startupConfig.GetValue<string?>("Token", null),
            ConnectionString = startupConfig.GetValue<string?>("ConnectionString", null),
            MainAdminRoleId = startupConfig.GetValue<ulong?>("MainAdminRoleId", null),
            RunWithCooldowns = startupConfig.GetValue<bool>("RunWithCooldowns", true),
            TestGuildId = startupConfig.GetValue<ulong?>("TestGuildId", null)
        };
    }

    public void Initialize()
    {
        Menu();
        WriteJsonConfig();
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
                    Console.WriteLine("\nclosing application with exit code 0");
                    Environment.Exit(0); //TODO check if this is the best way to do it
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
                $"[T] Bot token > {_botConfig.Token}\n" +
                $"[A] Main admin role id > {_botConfig.MainAdminRoleId}\n" +
                $"[S] Database connection string > {_botConfig.ConnectionString}\n" +
                $"[C] General cooldowns > {_botConfig.RunWithCooldowns}\n" +
                $"[X] Return\n");

#if DEBUG
            Console.WriteLine($"[D] DEBUG: Test guild id > {_botConfig.TestGuildId}\n");
#endif
            string? input = "";
            switch (Console.ReadKey().KeyChar)
            {
                case 'T':
                case 't':
                    input = WriteInput("Bot token");
                    if (ConfirmChanges(_botConfig.Token, input))
                        _botConfig.Token = input;
                    break;
                case 'A':
                case 'a':
                    input = WriteInput("Main admin role id");
                    if (ConfirmChanges(_botConfig.MainAdminRoleId.ToString(), input))
                        _botConfig.MainAdminRoleId =
                            ulong.TryParse(input, out var adminRoleId) ? adminRoleId : null;
                    break;
                case 'S':
                case 's':
                    input = WriteInput("Database connection string");
                    if (ConfirmChanges(_botConfig.ConnectionString, input))
                        _botConfig.ConnectionString = input;
                    break;
                case 'C':
                case 'c':
                    _botConfig.RunWithCooldowns = !_botConfig.RunWithCooldowns;

                    break;
                case 'X':
                case 'x':
                    return;
#if DEBUG
                case 'D':
                case 'd':
                    input = WriteInput("DEBUG: Test guild id");
                    if (ConfirmChanges(_botConfig.TestGuildId.ToString(), input))
                        _botConfig.TestGuildId = ulong.TryParse(input, out var testGuildId) ? testGuildId : null;
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

    private void WriteJsonConfig()
    {
        var serializerOptions = new JsonSerializerOptions()
            { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        var jsonString = JsonSerializer.Serialize(_botConfig, serializerOptions);
        File.WriteAllText($"{AppContext.BaseDirectory}config.json", jsonString);
    }

    private bool ConfirmChanges(string? oldValues, string? newValues)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine(
                "Confirm changes\n" +
                $"old: {oldValues}\n" +
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