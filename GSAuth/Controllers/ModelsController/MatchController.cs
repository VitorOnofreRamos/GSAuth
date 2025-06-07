using AutoMapper;
using GSAuth.DTOs;
using GSAuth.Models;
using GSAuth.Repositories;
using GSAuth.ML.Services;
using Microsoft.AspNetCore.Mvc;

namespace GSAuth.Controllers.ModelsController;

[Route("api/[controller]")]
[ApiController]
public class MatchController : ControllerBase
{
    private readonly _IRepository<Match> _repository;
    private readonly IMapper _mapper;

    // ADICIONAR estas dependências
    private readonly _IRepository<Need> _needRepository;
    private readonly _IRepository<Donation> _donationRepository;
    private readonly _IRepository<User> _userRepository;
    private readonly _IRepository<Organization> _organizationRepository;
    private readonly ICompatibilityMLService _compatibilityService;

    // ATUALIZAR o construtor
    public MatchController(
        _IRepository<Match> repository,
        IMapper mapper,
        _IRepository<Need> needRepository,
        _IRepository<Donation> donationRepository,
        _IRepository<User> userRepository,
        _IRepository<Organization> organizationRepository,
        ICompatibilityMLService compatibilityService)
    {
        _repository = repository;
        _mapper = mapper;
        _needRepository = needRepository;
        _donationRepository = donationRepository;
        _userRepository = userRepository;
        _organizationRepository = organizationRepository;
        _compatibilityService = compatibilityService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var need = await _repository.GetAll();
        return Ok(_mapper.Map<IEnumerable<MatchReadDto>>(need));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var need = await _repository.GetById(id);
        if (need == null)
            return NotFound();
        return Ok(_mapper.Map<MatchReadDto>(need));
    }

    [HttpPost]
    public async Task<IActionResult> Create(MatchCreateDto dto)
    {
        try
        {
            var match = _mapper.Map<Match>(dto);

            //Calcular CompatibilityScore automaticamente
            if (dto.CompatibilityScore == null || dto.CompatibilityScore == 0)
            {
                var compatibilityScore = await CalculateCompatibilityScore(dto.NeedId, dto.DonationId);
                match.CompatibilityScore = (int)Math.Round(compatibilityScore);
            }

            await _repository.Insert(match);

            return CreatedAtAction(
                nameof(GetById),
                new { id = match.Id },
                _mapper.Map<MatchReadDto>(match)
            );
        }
        catch (Exception ex)
        { 
            return BadRequest(new { message = $"Erro ao criar match: {ex.Message}" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, MatchCreateDto dto)
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

    [HttpPost("calculate-compatibility")]
    public async Task<IActionResult> CalculateCompatibility([FromBody] CalculateCompatibilityRequest request)
    {
        try
        {
            var compatibilityScore = await CalculateCompatibilityScore(request.NeedId, request.DonationId);

            return Ok(new
            {
                compatibilityScore = Math.Round(compatibilityScore, 2),
                needId = request.NeedId,
                donationId = request.DonationId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("train-model")]
    public async Task<IActionResult> TrainModel()
    {
        try
        {
            await _compatibilityService.TrainModelAsync();
            return Ok(new { message = "Modelo treinado com sucesso!" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Falha no treinamento: {ex.Message}" });
        }
    }

    [HttpGet("model-status")]
    public async Task<IActionResult> GetModelStatus()
    {
        try
        {
            var isModelTrained = await _compatibilityService.IsModelTrainedAsync();
            return Ok(new { isModelTrained });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    private async Task<float> CalculateCompatibilityScore(long needId, long donationId)
    {
        try
        {
            var need = await _needRepository.GetById(needId);
            var donation = await _donationRepository.GetById(donationId);
            var donor = await _userRepository.GetById(donation.DonorId);

            Organization organization = null;
            if (need.OrganizationId.HasValue)
            {
                try
                {
                    organization = await _organizationRepository.GetById(need.OrganizationId.Value);
                }
                catch (KeyNotFoundException)
                {
                    // Organização não encontrada, continua sem ela
                }
            }

            return await _compatibilityService.PredictCompatibilityAsync(need, donation, donor, organization);
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao calcular compatibilidade: {ex.Message}");
        }
    }
}

public class CalculateCompatibilityRequest
{
    public long NeedId { get; set; }
    public long DonationId { get; set; }
}