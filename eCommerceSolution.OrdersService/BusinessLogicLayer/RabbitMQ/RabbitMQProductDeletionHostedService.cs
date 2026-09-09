using Microsoft.Extensions.Hosting;

namespace eCommerce.OrderMicroservice.BussinessLogicLayer.RabbitMQ;

public class RabbitMQProductDeletionHostedService : IHostedService
{
    private readonly IRabbitMQProductDeletionConsumer _productDeletionConsumer;

    public RabbitMQProductDeletionHostedService(IRabbitMQProductDeletionConsumer consumer)
    {
        _productDeletionConsumer = consumer;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _productDeletionConsumer.Consume();

        return Task.CompletedTask;
        //Task.CompletedTask is a task that is already finished. It is used when a method must return a Task, but there is no asynchronous work to do.
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _productDeletionConsumer.Dispose();
        return Task.CompletedTask;
    }
}
