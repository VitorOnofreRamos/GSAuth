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
        return Ok(_mapper.Map<IEnumerable<OrganizationReadDTO>>(organization));
    }

    [HttpGet("{id}")]
    public async Task <IActionResult> GetById(long id)
    {
        var organization = await _repository.GetById(id);
        if (organization == null)
            return NotFound();
        return Ok(_mapper.Map<OrganizationReadDTO>(organization));
    }

    [HttpPost]
    public async Task<IActionResult> Create(OrganizationCreateDTO dto)
    {
        var organization = _mapper.Map<Organization>(dto);
        await _repository.Insert(organization);

        return CreatedAtAction(
            nameof(GetById),
            new { id = organization.Id },
            _mapper.Map<OrganizationReadDTO>(organization)
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, OrganizationCreateDTO dto)
    {
        var organization = await _repository.GetById(id);
        if (organization == null)
            return NotFound();

        _mapper.Map(dto, organization);
        await _repository.Update(organization);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _repository.Delete(id);
        return NoContent();
    }
}
