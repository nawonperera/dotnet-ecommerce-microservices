using AutoMapper;
using eCommerce.Core.DTO;
using eCommerce.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace eCommerce.Core.Mappers;

internal class RegisterRequestMappingProfile : Profile
{
    public RegisterRequestMappingProfile()
    {
        CreateMap<RegisterRequest, ApplicationUser>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
            .ForMember(dest => dest.PersonName, opt => opt.MapFrom(src => src.PersonName))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender));
    }
}
