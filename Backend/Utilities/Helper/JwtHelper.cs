using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Backend.Utilities.Helper;

public class JwtHelper
{
    public long GetMemberId(string jwtToken)
    {
        var token = jwtToken.Replace("Bearer", "").Trim();
        var payload = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var memberId = long.Parse(payload.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);

        return memberId;
    }

    public List<long> GetRoleId(string jwtToken)
    {
        var token = jwtToken.Replace("Bearer", "").Trim();
        var payload = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var result = payload.Claims
            .Where(x => x.Type == ClaimTypes.Role)
            .Select(x => long.Parse(x.Value))
            .ToList();

        return result ?? [];
    }
}
