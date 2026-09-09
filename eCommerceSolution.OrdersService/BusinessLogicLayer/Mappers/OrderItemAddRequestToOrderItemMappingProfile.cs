using AutoMapper;
using eCommerce.OrderMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrderMicroservice.DataAccessLayer.Entities;

namespace eCommerce.OrderMicroservice.BusinessLogicLayer.Mappers;

public class OrderItemAddRequestToOrderItemMappingProfile : Profile
{
    public OrderItemAddRequestToOrderItemMappingProfile()
    {
        CreateMap<OrderItemAddRequest, OrderItem>()
            .ForMember(destination => destination.ProductID, options => options.MapFrom(source => source.ProductID))
            .ForMember(destination => destination.UnitPrice, options => options.MapFrom(source => source.UnitPrice))
            .ForMember(destination => destination.Quantity, options => options.MapFrom(source => source.Quantity));
    }
}
