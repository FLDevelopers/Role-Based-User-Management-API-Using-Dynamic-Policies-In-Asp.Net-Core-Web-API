using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace WebApiWithRoleAuthentication.Authorization
{
    public class DynamicPolicyProvider : IAuthorizationPolicyProvider
    {
        private DefaultAuthorizationPolicyProvider BackupPolicyProvider { get; }
        public DynamicPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            BackupPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => BackupPolicyProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => BackupPolicyProvider.GetFallbackPolicyAsync();

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if(policyName.StartsWith("Role",StringComparison.OrdinalIgnoreCase))
            {
                var role = policyName["Role".Length..];
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new DynamicRoleRequirement(role));
                return Task.FromResult<AuthorizationPolicy?>(policy.Build());
            }
            return BackupPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}
