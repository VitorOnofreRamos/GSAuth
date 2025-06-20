﻿using AutoMapper;
using GSAuth.DTOs;
using GSAuth.Models;
using GSAuth.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GSAuth.Controllers.ModelsController;

[Route("api/[controller]")]
[ApiController]
public class NeedController : ControllerBase
{
    private readonly _IRepository<Need> _repository;
    private readonly IMapper _mapper;

    public NeedController(_IRepository<Need> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var need = await _repository.GetAll();
        return Ok(_mapper.Map<IEnumerable<NeedReadDTO>>(need));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var need = await _repository.GetById(id);
        if (need == null)
            return NotFound();
        return Ok(_mapper.Map<NeedReadDTO>(need));
    }

    [HttpPost]
    public async Task<IActionResult> Create(NeedCreateDTO dto)
    {
        var need = _mapper.Map<Need>(dto);
        await _repository.Insert(need);

        return CreatedAtAction(
            nameof(GetById),
            new { id = need.Id },
            _mapper.Map<NeedReadDTO>(need)
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, NeedCreateDTO dto)
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
