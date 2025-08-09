using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Backend.Utilities.Authentication;

public class SessionService(IConnectionMultiplexer _redis, IHostEnvironment _environment)
{
    private readonly IDatabase _redisDb = _redis.GetDatabase();

    public async Task StoreSessionAsync(string userId, JwtPayload data)
    {
        var hashEntries = new HashEntry[data.Count];
        for (int index = 0; index < data.Count; index++)
        {
            hashEntries[index] = new HashEntry(data.ElementAt(index).Key, JsonSerializer.Serialize(data.ElementAt(index).Value));
        }

        var key = $"session:{_environment.EnvironmentName.ToLower()}:{userId}";
        await _redisDb.HashSetAsync(key, hashEntries, CommandFlags.None);
        await _redisDb.KeyExpireAsync(key, TimeSpan.FromMinutes(30));
    }
    public async Task StoreSessionAsync(string userId, JwtPayload data, TimeSpan expiration)
    {
        var hashEntries = new HashEntry[data.Count];
        for (int index = 0; index < data.Count; index++)
        {
            hashEntries[index] = new HashEntry(data.ElementAt(index).Key, JsonSerializer.Serialize(data.ElementAt(index).Value));
        }

        var key = $"session:{_environment.EnvironmentName.ToLower()}:{userId}";
        await _redisDb.HashSetAsync(key, hashEntries, CommandFlags.None);
        await _redisDb.KeyExpireAsync(key, expiration);
    }

    public async Task<List<Claim>> GetSessionAsync(string userId)
    {
        var hashEntries = await _redisDb.HashGetAllAsync($"session:{_environment.EnvironmentName.ToLower()}:{userId}");
        var claims = new List<Claim>();

        foreach (var item in hashEntries)
        {
            claims.Add(new Claim(item.Name.ToString(), item.Value.ToString()));
        }

        return claims;
    }

    public async Task<bool> IsSessionExists(string name, string sessionId)
    {
        var redisValue = await _redisDb.HashGetAsync($"session:{_environment.EnvironmentName.ToLower()}:{name}", "sessionId");
        if (redisValue.IsNullOrEmpty) return false;
        var value = JsonSerializer.Deserialize<string>(redisValue.ToString());
        return value == sessionId;
    }

    public async Task<bool> RemoveSessionAsync(string name)
    {
        return await _redisDb.KeyDeleteAsync($"session:{_environment.EnvironmentName.ToLower()}:{name}");
    }
}
