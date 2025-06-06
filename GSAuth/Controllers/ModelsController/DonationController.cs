using AutoMapper;
using GSAuth.DTOs;
using GSAuth.Models;
using GSAuth.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GSAuth.Controllers.ModelsController;

[Route("api/[controller]")]
[ApiController]
public class DonationController : ControllerBase
{
    private readonly _IRepository<Donation> _repository;
    private readonly IMapper _mapper;

    public DonationController(_IRepository<Donation> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var need = await _repository.GetAll();
        return Ok(_mapper.Map<IEnumerable<DonationReadDto>>(need));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var need = await _repository.GetById(id);
        if (need == null)
            return NotFound();
        return Ok(_mapper.Map<DonationReadDto>(need));
    }

    [HttpPost]
    public async Task<IActionResult> Create(DonationCreateDto dto)
    {
        var need = _mapper.Map<Donation>(dto);
        await _repository.Insert(need);

        return CreatedAtAction(
            nameof(GetById),
            new { id = need.Id },
            _mapper.Map<DonationReadDto>(need)
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, DonationCreateDto dto)
    {
        var need = await _repository.GetById(id);
        if (need == null)
            return NotFound();

        _mapper.Map(dto, need);
        await _repository.Update(need);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _repository.Delete(id);
        return NoContent();
    }
}
