using eCommerce.OrderMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace eCommerce.OrderMicroservice.BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache;

    public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger, IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
    }

    public async Task<UserDTO?> GetUserByUserID(Guid userID)
    {
        try
        {

            string cacheKeyToRead = $"user:{userID}";
            string? cachedUser = await _distributedCache.GetStringAsync(cacheKeyToRead);

            if (cachedUser != null)
            {
                UserDTO? userFromCache = JsonSerializer.Deserialize<UserDTO>(cachedUser);
                return userFromCache;
            }

            HttpResponseMessage response = await _httpClient.GetAsync($"/gateway/users/{userID}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    UserDTO? userFromFallback = await response.Content.ReadFromJsonAsync<UserDTO>();

                    if (userFromFallback == null)
                    {
                        throw new NotImplementedException("Fallback policy was not implemented.");
                    }

                    return userFromFallback;
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Bad request", null, HttpStatusCode.BadRequest);
                }
                else
                {
                    //throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
                    return new UserDTO(
                        PersonName: "Temporarily Unavailable",
                        Email: "Temporarily Unavailable",
                        Gender: "Temporarily Unavailable",
                        UserID: Guid.Empty);
                }
            }

            UserDTO? user = await response.Content.ReadFromJsonAsync<UserDTO>();

            if (user == null)
            {
                throw new ArgumentException("Invalid User ID");
            }

            //Store the user data (retrieved from response) into cache
            string userJson = JsonSerializer.Serialize(user);

            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(3));

            string cacheKey = $"user:{userID}";

            await _distributedCache.SetStringAsync(cacheKey, userJson, options);


            return user;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "Request failed because of circuit breaker is in open state. Running dummy data");
            return new UserDTO(
                        PersonName: "Temporarily Unavailable (Circuit Breaker)",
                        Email: "Temporarily Unavailable (Circuit Breaker)",
                        Gender: "Temporarily Unavailable (Circuit Breaker)",
                        UserID: Guid.Empty);
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex, "Request failed because of circuit breaker is in open state. Running dummy data");
            return new UserDTO(
                        PersonName: "Temporarily Unavailable (Timeout)",
                        Email: "Temporarily Unavailable (Timeout)",
                        Gender: "Temporarily Unavailable (Timeout)",
                        UserID: Guid.Empty);
        }
    }
}
