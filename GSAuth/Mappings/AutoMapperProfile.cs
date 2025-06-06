using AutoMapper;
using GSAuth.Models;
using GSAuth.DTOs;

namespace GSAuth.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile() {
        CreateMap<Organization, OrganizationReadDTO>();
        CreateMap<OrganizationCreateDTO, Organization>();

        CreateMap<User, UserDTO>();
        CreateMap<UserDTO, User>();

        CreateMap<Need, NeedReadDTO>();
        CreateMap<NeedCreateDTO, Need>();

        CreateMap<Donation, DonationReadDto>();
        CreateMap<DonationCreateDto, Donation>();

        CreateMap<Match, MatchReadDto>();
        CreateMap<MatchCreateDto, Match>();
    }
}
