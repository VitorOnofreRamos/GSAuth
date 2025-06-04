using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GSAuth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpGet("public")]
    public ActionResult<object> PublicEndpoint()
    {
        return Ok(new
        {
            message = "Este endpoint é público - JWT funcionando!",
            timestamp = DateTime.Now,
            success = true
        });
    }

    [HttpGet("protected")]
    [Authorize]
    public ActionResult<object> ProtectedEndpoint()
    {
        try
        {
            // Debug detalhado
            _logger.LogInformation("=== ENDPOINT PROTEGIDO ACESSADO ===");
            _logger.LogInformation("IsAuthenticated: {IsAuth}", User.Identity?.IsAuthenticated);
            _logger.LogInformation("AuthenticationType: {AuthType}", User.Identity?.AuthenticationType);
            _logger.LogInformation("Name: {Name}", User.Identity?.Name);
            _logger.LogInformation("Claims Count: {Count}", User.Claims.Count());

            // Extrair claims importantes
            var claims = User.Claims.ToDictionary(c => c.Type, c => c.Value);
            var userId = User.FindFirst("user_id")?.Value;
            var email = User.FindFirst("email")?.Value;
            var name = User.FindFirst("name")?.Value;
            var role = User.FindFirst("role")?.Value;

            _logger.LogInformation("Extracted - UserId: {UserId}, Email: {Email}, Name: {Name}, Role: {Role}",
                userId, email, name, role);

            // Log todas as claims para debug
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation("Claim - Type: {Type}, Value: {Value}", claim.Type, claim.Value);
            }

            return Ok(new
            {
                message = "✅ TOKEN VÁLIDO! Acesso autorizado com sucesso!",
                user = new
                {
                    id = userId,
                    email = email,
                    name = name,
                    role = role
                },
                authentication = new
                {
                    isAuthenticated = User.Identity?.IsAuthenticated,
                    authenticationType = User.Identity?.AuthenticationType,
                    identityName = User.Identity?.Name
                },
                allClaims = claims,
                timestamp = DateTime.Now,
                success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no endpoint protegido");
            return StatusCode(500, new { message = "Erro interno", error = ex.Message });
        }
    }

    [HttpGet("headers")]
    public ActionResult<object> CheckHeaders()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        var hasBearer = authHeader?.StartsWith("Bearer ") == true;
        var tokenPart = hasBearer ? authHeader.Substring(7) : null;

        return Ok(new
        {
            hasAuthorizationHeader = !string.IsNullOrEmpty(authHeader),
            authorizationHeader = authHeader,
            hasBearer = hasBearer,
            tokenLength = tokenPart?.Length,
            tokenPreview = tokenPart?.Substring(0, Math.Min(tokenPart.Length, 50)) + "...",
            allHeaders = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
        });
    }

    [HttpGet("test-claims")]
    [Authorize]
    public ActionResult<object> TestClaims()
    {
        var userIdMethods = new Dictionary<string, string>
        {
            ["user_id_claim"] = User.FindFirst("user_id")?.Value,
            ["sub_claim"] = User.FindFirst("sub")?.Value,
            ["nameidentifier_claim"] = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            ["identity_name"] = User.Identity?.Name
        };

        return Ok(new
        {
            message = "Testando diferentes formas de extrair user_id",
            methods = userIdMethods,
            allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList(),
            recommendation = "Use 'user_id' claim para melhor compatibilidade"
        });
    }
}