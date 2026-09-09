using AutoMapper;
using eCommerce.OrderMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrderMicroservice.DataAccessLayer.Entities;

namespace eCommerce.OrderMicroservice.BusinessLogicLayer.Mappers;

public class OrderUpdateRequestToOrderMappingProfile : Profile
{
    public OrderUpdateRequestToOrderMappingProfile()
    {
        CreateMap<OrderUpdateRequest, Order>()
          .ForMember(destination => destination.OrderID, options => options.MapFrom(source => source.OrderID))
          .ForMember(destination => destination.UserID, options => options.MapFrom(source => source.UserID))
          .ForMember(destination => destination.OrderDate, options => options.MapFrom(source => source.OrderDate))
          .ForMember(destination => destination.OrderItems, options => options.MapFrom(source => source.OrderItems))
          .ForMember(destination => destination._id, options => options.Ignore())
          .ForMember(destination => destination.TotalBill, options => options.Ignore());
    }
}
