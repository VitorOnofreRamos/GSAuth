using AutoMapper;
using GSAuth.Models;
using GSAuth.DTOs;

namespace GSAuth.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile() {
        CreateMap<User, UserDTO>();
        CreateMap<UserDTO, User>();
    }
}
