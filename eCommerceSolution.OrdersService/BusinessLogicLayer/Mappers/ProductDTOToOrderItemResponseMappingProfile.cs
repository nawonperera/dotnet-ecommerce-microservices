using AutoMapper;
using eCommerce.OrderMicroservice.BusinessLogicLayer.DTO;

namespace eCommerce.OrderMicroservice.BusinessLogicLayer.Mappers;

public class ProductDTOToOrderItemResponseMappingProfile:Profile
{
    public ProductDTOToOrderItemResponseMappingProfile()
    {
        CreateMap<ProductDTO, OrderItemResponse>()
          .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
          .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));
    }
}
