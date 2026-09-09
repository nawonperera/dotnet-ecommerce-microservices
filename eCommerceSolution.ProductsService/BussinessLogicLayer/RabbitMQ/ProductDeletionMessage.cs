namespace eCommerce.BussinessLogicLayer.RabbitMQ;

public record ProductDeletionMessage(Guid ProductID, string? ProductName);