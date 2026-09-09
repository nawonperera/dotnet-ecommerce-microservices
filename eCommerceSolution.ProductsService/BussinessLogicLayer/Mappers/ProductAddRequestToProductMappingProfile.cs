using AutoMapper;
using eCommerce.BusinessLogicLayer.DTO;
using eCommerce.DataAccessLayer.Entities;

namespace eCommerce.BusinessLogicLayer.Mappers;

public class ProductAddRequestToProductMappingProfile : Profile
{
    public ProductAddRequestToProductMappingProfile()
    {
        CreateMap<ProductAddRequest, Product>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName));
        CreateMap<ProductAddRequest, Product>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));
        CreateMap<ProductAddRequest, Product>()
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice));
        CreateMap<ProductAddRequest, Product>()
            .ForMember(dest => dest.QuantityInStock, opt => opt.MapFrom(src => src.QuantityInStock));
        CreateMap<ProductAddRequest, Product>()
            .ForMember(dest => dest.ProductId, opt => opt.Ignore());
    }
}
