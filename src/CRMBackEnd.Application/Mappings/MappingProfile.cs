using AutoMapper;
using CRMBackEnd.Application.DTOs;
using CRMBackEnd.Domain.Entities;

namespace CRMBackEnd.Application.Mappings;

/// <summary>
/// AutoMapper profile for mapping between entities and DTOs
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Customer, CustomerInfoResponse>();
    }
}
