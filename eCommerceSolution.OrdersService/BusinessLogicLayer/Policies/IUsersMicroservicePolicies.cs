using Polly;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public interface IUsersMicroservicePolicy
{
    IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy();
}
