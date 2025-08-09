using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Backend.Utilities.Helper;

public class JwtHelper
{
    public long GetUserId(string jwtToken)
    {
        var token = jwtToken.Replace("Bearer", "").Trim();
        var payload = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var userId = long.Parse(payload.Claims.First(x => x.Type == "userId").Value);

        return userId;
    }

    public List<long> GetRoleId(string jwtToken)
    {
        var token = jwtToken.Replace("Bearer", "").Trim();
        var payload = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var result = payload.Claims
            .Where(x => x.Type == "role")
            .Select(x => long.Parse(x.Value))
            .ToList();

        return result ?? [];
    }
}
