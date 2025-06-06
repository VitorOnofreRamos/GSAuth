using AutoMapper;
using GSAuth.Models;
using GSAuth.DTOs;

namespace GSAuth.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile() {
        CreateMap<Organization, OrganizationDTO>();
        CreateMap<OrganizationDTO, Organization>();

        CreateMap<User, UserDTO>();
        CreateMap<UserDTO, User>();

        CreateMap<Need, NeedDTO>();
        CreateMap<NeedDTO, Need>();

        CreateMap<Donation, DonationDto>();
        CreateMap<DonationDto, Donation>();

        CreateMap<Match, MatchDto>();
        CreateMap<MatchDto, Match>();
    }
}
