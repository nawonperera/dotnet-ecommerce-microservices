using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class PollyPolicies : IPollyPolicies
{
    private readonly ILogger<UsersMicroservicePolicy> _logger;

    public PollyPolicies(ILogger<UsersMicroservicePolicy> logger)
    {
        _logger = logger;
    }



    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
    {

        AsyncRetryPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(res => !res.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    retryCount: retryCount, //Number of retries
                    sleepDurationProvider: (retryAttempt) => TimeSpan.FromSeconds(Math.Pow(2,retryAttempt)), // Delay between each retry
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        // TO DO: add logs
                        // Log the retry attempt, delay, and reason for the retry.
                        // Example: "Retry 2 after 2 seconds because the request returned HTTP 500."
                        _logger.LogInformation($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds");
                    });

        /*
         Request fails → onRetry runs → wait 2 seconds → retry
         Request fails → onRetry runs → wait 4 seconds → retry
         Request succeeds → stop

        outcome = Why the request failed — exception or HTTP response
        timespan = How long Polly will wait before retrying
        retryAttempt = Which retry this is: 1, 2, 3...
        context = Extra information/context associated with the request
         
         */

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int handledEventsAllowedBeforeBreaking,TimeSpan durationOfBreak)
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(res => !res.IsSuccessStatusCode)
    .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: handledEventsAllowedBeforeBreaking, //After 3 continous faled requests the circuit breaker gets open
                durationOfBreak: durationOfBreak, //after how much time the CB should be state from open state to half-open
                onBreak: (outcome, timespan) => // This message will be printed when moving to closing state to open state.
                {
                    _logger.LogInformation($"Circuit Breaker open for {timespan.TotalMinutes} minutes due to consecutive 3 failures. The subsequent requests will be blocked");
                },
                onReset: () => //  This message will be printed when moving to half state to closed state.
                {
                    _logger.LogInformation($"Circuit breaker closed. The subsequent requests will be allowed.");
                });
        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan timeout)
    {
        AsyncTimeoutPolicy<HttpResponseMessage> policy = Policy.TimeoutAsync<HttpResponseMessage>(timeout);

        return policy;
    }

}
