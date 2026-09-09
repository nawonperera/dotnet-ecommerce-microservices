

namespace eCommerce.BussinessLogicLayer.RabbitMQ;

public record ProductNameUpdateMessage(Guid ProductID, string? NewName);
