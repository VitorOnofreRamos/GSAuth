using GSAuth.DTOs;
using GSAuth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

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

            _logger.LogInformation("Usuário registrado com sucesso: {Email}", registerDto.Email);

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

            _logger.LogInformation("Login realizado com sucesso: {Email}", loginDto.Email);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Tentativa de login não autorizada para email: {Email}", loginDto.Email);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno no login");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDto)
    {
        try
        {
            // Debug de autorização
            _logger.LogInformation("Tentativa de alteração de senha - IsAuthenticated: {IsAuth}", User.Identity?.IsAuthenticated);
            _logger.LogInformation("Claims count: {ClaimsCount}", User.Claims.Count());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                _logger.LogWarning("Token inválido - user_id não encontrado nas claims");
                return Unauthorized(new { message = "Token inválido" });
            }

            _logger.LogInformation("Alterando senha para usuário ID: {UserId}", userId);

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
            // Debug de autorização
            _logger.LogInformation("Buscando usuário atual - IsAuthenticated: {IsAuth}", User.Identity?.IsAuthenticated);
            _logger.LogInformation("Auth Type: {AuthType}", User.Identity?.AuthenticationType);

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                _logger.LogWarning("Token inválido - user_id não encontrado nas claims");
                LogAllClaims(); // Para debug
                return Unauthorized(new { message = "Token inválido" });
            }

            _logger.LogInformation("Buscando dados do usuário ID: {UserId}", userId);

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

    [HttpDelete("delete-account")]
    [Authorize]
    public async Task<ActionResult> DeleteMyAccount([FromBody] DeleteAccountDTO deleteAccountDto)
    {
        try
        {
            _logger.LogInformation("Tentativa de exclusão de conta - IsAuthenticated: {IsAuth}", User.Identity?.IsAuthenticated);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                _logger.LogWarning("Token inválido - user_id não encontrado nas claims");
                return Unauthorized(new { message = "Token inválido" });
            }

            _logger.LogInformation("Deletando conta do usuário ID: {UserId}", userId);

            var success = await _authService.DeleteAccountAsync(userId, deleteAccountDto.Password);
            if (!success)
            {
                return BadRequest(new { message = "Senha incorreta ou usuário não encontrado" });
            }

            _logger.LogInformation("Conta deletada com sucesso para usuário ID: {UserId}", userId);

            return Ok(new { message = "Conta deletada com sucesso" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Tentativa não autorizada de exclusão de conta para usuário: {UserId}", GetCurrentUserId());
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno na exclusão de conta");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    //[HttpDelete("admin/delete-user/{userId}")]
    //[Authorize(Roles = "ADMIN")]
    //public async Task<ActionResult> DeleteUserByAdmin(long userId)
    //{
    //    try
    //    {
    //        _logger.LogInformation("Admin tentando deletar usuário ID: {UserId}", userId);

    //        var currentUserId = GetCurrentUserId();
    //        if (currentUserId == userId)
    //        {
    //            return BadRequest(new { message = "Administradores não podem deletar sua própria conta por este endpoint. Use delete-account." });
    //        }

    //        var success = await _authService.DeleteUserByAdminAsync(userId);
    //        if (!success)
    //        {
    //            return NotFound(new { message = "Usuário não encontrado" });
    //        }

    //        _logger.LogInformation("Usuário ID: {UserId} deletado pelo admin ID: {AdminId}", userId, currentUserId);

    //        return Ok(new { message = "Usuário deletado com sucesso" });
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Erro interno na exclusão de usuário pelo admin");
    //        return StatusCode(500, new { message = "Erro interno do servidor" });
    //    }
    //}

    private long GetCurrentUserId()
    {
        // Tentar múltiplas formas de obter o user_id
        var userIdClaim = User.FindFirst("user_id")?.Value
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        _logger.LogInformation("Tentando extrair user_id das claims. Valor encontrado: {UserIdClaim}", userIdClaim);

        if (long.TryParse(userIdClaim, out long userId))
        {
            return userId;
        }

        _logger.LogWarning("Não foi possível extrair user_id válido das claims");
        return 0;
    }

    private void LogAllClaims()
    {
        _logger.LogInformation("=== DEBUG: Todas as Claims ===");
        foreach (var claim in User.Claims)
        {
            _logger.LogInformation("Claim - Type: {Type}, Value: {Value}", claim.Type, claim.Value);
        }
        _logger.LogInformation("=== FIM DEBUG Claims ===");
    }
}

public class ValidateTokenDTO
{
    public string Token { get; set; }
}