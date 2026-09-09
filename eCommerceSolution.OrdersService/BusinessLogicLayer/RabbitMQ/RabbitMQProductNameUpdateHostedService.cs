using Microsoft.Extensions.Hosting;

namespace eCommerce.OrderMicroservice.BussinessLogicLayer.RabbitMQ;

public class RabbitMQProductNameUpdateHostedService : IHostedService
{
    private readonly IRabbitMQProductNameUpdateConsumer _productNameUpdateConsumer;

    public RabbitMQProductNameUpdateHostedService(IRabbitMQProductNameUpdateConsumer consumer)
    {
        _productNameUpdateConsumer = consumer;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _productNameUpdateConsumer.Consume();

        return Task.CompletedTask;
        //Task.CompletedTask is a task that is already finished. It is used when a method must return a Task, but there is no asynchronous work to do.
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _productNameUpdateConsumer.Dispose();
        return Task.CompletedTask;
    }
}
