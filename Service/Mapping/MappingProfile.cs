using AutoMapper;
using Core.DTOs.Auth;
using Core.DTOs.Cars;
using Core.DTOs.Users;
using Core.Entities;

namespace Service.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RegisterRequest, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Rentals, opt => opt.Ignore());

        CreateMap<UpdateUserProfileRequest, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Rentals, opt => opt.Ignore());

        CreateMap<User, UserProfileResponse>();

        CreateMap<CreateCarRequest, Car>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Rentals, opt => opt.Ignore());

        CreateMap<UpdateCarRequest, Car>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Rentals, opt => opt.Ignore());

        CreateMap<Car, CarResponse>();
    }
}
