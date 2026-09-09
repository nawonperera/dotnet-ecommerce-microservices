namespace eCommerce.OrderMicroservice.BussinessLogicLayer.RabbitMQ;

public record ProductNameUpdateMessage(Guid ProductID, string? NewName);
