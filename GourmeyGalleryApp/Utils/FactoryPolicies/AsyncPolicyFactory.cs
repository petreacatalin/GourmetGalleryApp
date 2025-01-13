using Polly;

namespace GourmeyGalleryApp.Utils.FactoryPolicies
{
    public class AsyncPolicyFactory : IAsyncPolicyFactory
    {
        private readonly IDictionary<string, IAsyncPolicy> _policies;

        public AsyncPolicyFactory(IDictionary<string, IAsyncPolicy> policies)
        {
            _policies = policies ?? throw new ArgumentNullException(nameof(policies));
        }

        public IAsyncPolicy GetPolicy(string policyName)
        {
            if (_policies.TryGetValue(policyName, out var policy))
            {
                return policy;
            }

            throw new KeyNotFoundException($"Policy with name '{policyName}' not found.");
        }
    }
}
