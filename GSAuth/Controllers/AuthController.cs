using GSAuth.DTOs;
using GSAuth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GSAuth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDTO>> Register([FromBody] RegisterDTO registerDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(registerDto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação no registro");
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argumento inválido no registro");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno no registro");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDTO>> Login([FromBody] LoginDTO loginDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(loginDto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Tentativa de login não autorizada para email: {Email}", loginDto.Email);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno do login");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            var success = await _authService.ChangePasswordAsync(userId, changePasswordDto);
            if (!success)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            return Ok(new { message = "Senha alterada com sucesso" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Tentativa não autorizada de alteração de senha para usuário: {UserId}", GetCurrentUserId());
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno na alteração de senha");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDTO>> GetCurrentUser()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            var user = await _authService.GetCurrentUserAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar usuário atual");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpPost("validate-token")]
    public async Task<ActionResult> ValidateToken([FromBody] ValidateTokenDTO validateTokenDto)
    {
        try
        {
            var isValid = await _authService.ValidateTokenAsync(validateTokenDto.Token);
            return Ok(new { isValid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na validação do token");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    private long GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("user_id")?.Value;
        if (long.TryParse(userIdClaim, out long userId))
        {
            return userId;
        }
        return 0;
    }
}

// DTO adicional para validação de token
public class ValidateTokenDTO
{
    public string Token { get; set; }
}
