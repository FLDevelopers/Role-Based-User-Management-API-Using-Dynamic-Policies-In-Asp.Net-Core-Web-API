using Microsoft.AspNetCore.Authorization;

namespace WebApiWithRoleAuthentication.Authorization
{
    public class DynamicRoleHanlder : AuthorizationHandler<DynamicRoleRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DynamicRoleRequirement requirement)
        {
            if (context.User.IsInRole(requirement.RoleName))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }

    public class DynamicRoleRequirement : IAuthorizationRequirement
    {
        public string RoleName { get; }
        public DynamicRoleRequirement(string roleName)
        {
            RoleName = roleName;
        }
    }
}
