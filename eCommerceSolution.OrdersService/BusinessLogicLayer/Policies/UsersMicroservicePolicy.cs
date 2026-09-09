using Microsoft.Extensions.Logging;
using Polly;
using Polly.Wrap;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class UsersMicroservicePolicy : IUsersMicroservicePolicy
{
    private readonly ILogger<UsersMicroservicePolicy> _logger;
    private readonly IPollyPolicies _pollyPolicies;

    public UsersMicroservicePolicy(ILogger<UsersMicroservicePolicy> logger, IPollyPolicies pollyPolicies)
    {
        _logger = logger;
        _pollyPolicies = pollyPolicies;
    }


    public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        var retryPolicy = _pollyPolicies.GetRetryPolicy(5);
        var circuitBreakerPolicy = _pollyPolicies.GetCircuitBreakerPolicy(3, TimeSpan.FromMinutes(2));
        var timeoutPolicy = _pollyPolicies.GetTimeoutPolicy(TimeSpan.FromSeconds(5));

        AsyncPolicyWrap<HttpResponseMessage> wrappedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);

        return wrappedPolicy;
    }
}
