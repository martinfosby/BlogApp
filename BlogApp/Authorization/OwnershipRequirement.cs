using Microsoft.AspNetCore.Authorization;


namespace BlogApp.Authorization
{
    public class OwnershipRequirement : IAuthorizationRequirement
    {
        // empty - marker requirement
    }
}