using AutoMapper;
using eCommerce.OrderMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrderMicroservice.DataAccessLayer.Entities;
namespace eCommerce.OrderMicroservice.BusinessLogicLayer.Mappers;

public class OrderAddRequestToOrderMappingProfile:Profile
{
    public OrderAddRequestToOrderMappingProfile()
    {
        CreateMap<OrderAddRequest, Order>()
            .ForMember(destination => destination.UserID, options => options.MapFrom(source => source.UserID))
            .ForMember(destination => destination.OrderDate, options => options.MapFrom(source => source.OrderDate))
            .ForMember(destination => destination.OrderItems, options => options.MapFrom(source => source.OrderItems))
            .ForMember(destination => destination.OrderID, options => options.Ignore())
            .ForMember(destination => destination._id, options => options.Ignore())
            .ForMember(destination => destination.TotalBill, options => options.Ignore());
    }
}
