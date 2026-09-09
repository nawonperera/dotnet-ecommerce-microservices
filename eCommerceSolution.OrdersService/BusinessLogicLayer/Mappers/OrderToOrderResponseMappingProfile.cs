using AutoMapper;
using eCommerce.OrderMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrderMicroservice.DataAccessLayer.Entities;

namespace eCommerce.OrderMicroservice.BusinessLogicLayer.Mappers;

public class OrderToOrderResponseMappingProfile : Profile
{
    public OrderToOrderResponseMappingProfile()
    {
        CreateMap<Order, OrderResponse>()
          .ForMember(destination => destination.OrderID, options => options.MapFrom(source => source.OrderID))
          .ForMember(destination => destination.UserID, options => options.MapFrom(source => source.UserID))
          .ForMember(destination => destination.OrderDate, options => options.MapFrom(source => source.OrderDate))
          .ForMember(destination => destination.OrderItems, options => options.MapFrom(source => source.OrderItems))
          .ForMember(destination => destination.TotalBill, options => options.MapFrom(source => source.TotalBill));
    }
}