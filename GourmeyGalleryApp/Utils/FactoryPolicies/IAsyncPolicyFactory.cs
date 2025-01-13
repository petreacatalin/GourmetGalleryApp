using Polly;

namespace GourmeyGalleryApp.Utils.FactoryPolicies
{
    public interface IAsyncPolicyFactory
    {
        IAsyncPolicy GetPolicy(string policyName);
    }
}
