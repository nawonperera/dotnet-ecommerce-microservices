namespace eCommerce.OrderMicroservice.BussinessLogicLayer.RabbitMQ;

public interface IRabbitMQProductDeletionConsumer
{
    void Consume();
    void Dispose();
}
