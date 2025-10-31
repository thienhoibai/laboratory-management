using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Security.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string PermissionName { get; }
        public PermissionRequirement(string permissionName) => PermissionName = permissionName;
    }

    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // permission claim type "perm" populated in JWT
            if (context.User.HasClaim(c => c.Type == "perm" && c.Value == requirement.PermissionName))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
