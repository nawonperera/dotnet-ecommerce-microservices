namespace eCommerce.BussinessLogicLayer.RabbitMQ;

public interface IRabbitMQPublisher
{
    public void Publish<T>(Dictionary<string, object> headers, T message);
}
