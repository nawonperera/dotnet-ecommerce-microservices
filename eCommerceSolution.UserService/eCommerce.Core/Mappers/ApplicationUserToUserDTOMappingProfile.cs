using AutoMapper;
using eCommerce.Core.DTO;
using eCommerce.Core.Entities;

namespace eCommerce.Core.Mappers;

internal class ApplicationUserToUserDTOMappingProfile:Profile
{
    public ApplicationUserToUserDTOMappingProfile()
    {
        CreateMap<ApplicationUser, UserDTO>()
            .ForMember(destination => destination.UserID, options => options.MapFrom(source => source.UserId))
            .ForMember(destination => destination.Email, options => options.MapFrom(source => source.Email))
            .ForMember(destination => destination.PersonName, options => options.MapFrom(source => source.PersonName))
            .ForMember(destination => destination.Gender, options => options.MapFrom(source => source.Gender));
    }
}
