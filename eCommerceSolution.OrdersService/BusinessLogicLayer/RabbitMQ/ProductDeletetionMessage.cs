namespace eCommerce.ProductsService.BussinessLogicLayer.RabbitMQ;

public record ProductDeletionMessage(Guid ProductID, string? ProductName);