using Microsoft.AspNetCore.Authorization;

namespace Backend.Utilities.Authorization;

public class CrudAuthorizationRequirement(string _requirement) : IAuthorizationRequirement, IAuthorizationRequirementData
{
    public string Requirement {get;set;} = _requirement;
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return this;
    }
}
