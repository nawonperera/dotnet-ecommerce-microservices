

namespace eCommerce.OrderMicroservice.BussinessLogicLayer.RabbitMQ;

public interface IRabbitMQProductNameUpdateConsumer
{
    void Consume();
    void Dispose();
}
