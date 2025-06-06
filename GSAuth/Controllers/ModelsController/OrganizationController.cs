using AutoMapper;
using GSAuth.DTOs;
using GSAuth.Models;
using GSAuth.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GSAuth.Controllers.ModelsController;

[Route("api/[controller]")]
[ApiController]
public class OrganizationController : ControllerBase
{
    private readonly _IRepository<Organization> _repository;
    private readonly IMapper _mapper;

    public OrganizationController(_IRepository<Organization> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var organization = await _repository.GetAll();
        return Ok(_mapper.Map<IEnumerable<OrganizationDTO>>(organization));
    }

    [HttpGet]
}
