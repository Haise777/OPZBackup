using Discord.WebSocket;

namespace OPZBackup;

public class RateLimiter
{
    private readonly Dictionary<ulong, DateTime> _usersTimestamps = new();
    private readonly Dictionary<ulong, int> _rateLimitedUsers = new();

    public async Task RateLimitAsync(ulong userId)
    {
        if (!_usersTimestamps.ContainsKey(userId))
        {
            _usersTimestamps.Add(userId, DateTime.Now);
            _rateLimitedUsers.Add(userId, 1);
            return;
        }

        var lastAttempt = _usersTimestamps[userId];
        var timeSinceLastAttempt = DateTime.Now - lastAttempt;

        if (timeSinceLastAttempt < TimeSpan.FromSeconds(CooldownFromAttemptsN(_rateLimitedUsers[userId])))
        {
            _rateLimitedUsers[userId]++;
            await ApplyDelay(userId);
        }
        else
        {
            _rateLimitedUsers[userId] = 1;
        }

        CleanupStagnantEntries();
        _usersTimestamps[userId] = DateTime.Now;
    }

    public bool IsRateLimitedAsync(ulong userId)
    {
        if (!_rateLimitedUsers.TryGetValue(userId, out var attemptsNumber))
            return false;

        return attemptsNumber > 0;
    }

    private async Task ApplyDelay(ulong userId)
    {
        switch (_rateLimitedUsers[userId])
        {
            case 1:
                await Task.Delay(1000);
                break;
            case 2:
                await Task.Delay(2000);
                break;
            case 3:
                await Task.Delay(5000);
                break;
            default:
                await Task.Delay(10000);
                break;
        }
    }

    private void CleanupStagnantEntries()
    {
        if (_usersTimestamps.Count <= 100) return;

        foreach (var entry in _usersTimestamps)
        {
            if (DateTime.Now - entry.Value > TimeSpan.FromMinutes(1))
            {
                _usersTimestamps.Remove(entry.Key);
            }
        }
    }

    private static int CooldownFromAttemptsN(int attempts) => attempts switch
    {
        1 => 1,
        2 => 2,
        3 => 5,
        _ => 10
    };
}