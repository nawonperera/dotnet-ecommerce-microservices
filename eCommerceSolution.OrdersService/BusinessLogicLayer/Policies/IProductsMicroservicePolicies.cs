using Polly;
using System;
using System.Collections.Generic;
using System.Text;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public interface IProductsMicroservicePolicies
{
    public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy();
    public IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy();
}
