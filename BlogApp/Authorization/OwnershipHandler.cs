using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using BlogApp.Models;
using BlogApp.Data;
using Microsoft.EntityFrameworkCore;


namespace BlogApp.Authorization
{
    public class OwnershipHandler<T> : AuthorizationHandler<OwnershipRequirement, T> where T : class
    {
        private readonly BlogAppContext _db;
        private readonly IHttpContextAccessor _http;


        public OwnershipHandler(BlogAppContext db, IHttpContextAccessor http)
        {
            _db = db;
            _http = http;
        }


        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OwnershipRequirement requirement, T resource)
        {
            // resource is expected to be an entity that has OwnerId property
            if (!_http.HttpContext.User.Identity.IsAuthenticated)
            {
                return;
            }


            var userIdClaim = _http.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return;


            if (resource == null) return;


            var ownerIdProp = resource.GetType().GetProperty("OwnerId");
            if (ownerIdProp == null) return;


            var ownerIdVal = ownerIdProp.GetValue(resource);
            if (ownerIdVal == null) return;


            if (Guid.TryParse(ownerIdVal.ToString(), out var ownerGuid))
            {
                if (ownerGuid.ToString() == userIdClaim) context.Succeed(requirement);
            }
        }
    }
}