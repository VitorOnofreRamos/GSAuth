//using GSAuth.DTOs;
//using GSAuth.Services;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using System.ComponentModel.DataAnnotations;

//namespace GSAuth.Controllers;

//[ApiController]
//[Route("api/[controller]")]
//[Authorize(Roles = "ADMIN")]
//public class UserManagementController : ControllerBase
//{
//    private readonly IUserService _userService;
//    private readonly ILogger<UserManagementController> _logger;

//    public UserManagementController(IUserService userService, ILogger<UserManagementController> logger)
//    {
//        _userService = userService;
//        _logger = logger;
//    }

//    [HttpGet("users")]
//    public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllUsers()
//    {
//        try
//        {
//            var users = await _userService.GetAllAsync();
//            var userDtos = users.Select(u => new UserDTO
//            {
//                Id = u.Id,
//                Email = u.Email,
//                Phone = u.Phone,
//                Name = u.Name,
//                Role = u.Role,
//                IsActive = u.IsActive,
//                LastLogin = u.LastLogin,
//                CreatedAt = u.CreatedAt,
//                UpdatedAt = u.UpdatedAt,
//                OrganizationId = u.OrganizationId
//            });

//            _logger.LogInformation("Admin listou todos os usuários. Total: {Count}", userDtos.Count());

//            return Ok(userDtos);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Erro ao listar usuários");
//            return StatusCode(500, new { message = "Erro interno do servidor" });
//        }
//    }

//    [HttpGet("users/{userId}")]
//    public async Task<ActionResult<UserDTO>> GetUserById(long userId)
//    {
//        try
//        {
//            var user = await _userService.GetByIdAsync(userId);
//            if (user == null)
//            {
//                return NotFound(new { message = "Usuário não encontrado" });
//            }

//            var userDto = new UserDTO
//            {
//                Id = user.Id,
//                Email = user.Email,
//                Phone = user.Phone,
//                Name = user.Name,
//                Role = user.Role,
//                IsActive = user.IsActive,
//                LastLogin = user.LastLogin,
//                CreatedAt = user.CreatedAt,
//                UpdatedAt = user.UpdatedAt,
//                OrganizationId = user.OrganizationId
//            };

//            return Ok(userDto);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Erro ao buscar usuário {UserId}", userId);
//            return StatusCode(500, new { message = "Erro interno do servidor" });
//        }
//    }

//    [HttpPut("users/{userId}/activate")]
//    public async Task<ActionResult> ActivateUser(long userId)
//    {
//        try
//        {
//            var user = await _userService.GetByIdAsync(userId);
//            if (user == null)
//            {
//                return NotFound(new { message = "Usuário não encontrado" });
//            }

//            user.IsActive = "Y";
//            await _userService.UpdateAsync(user);

//            _logger.LogInformation("Usuário {UserId} ativado pelo admin", userId);

//            return Ok(new { message = "Usuário ativado com sucesso" });
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Erro ao ativar usuário {UserId}", userId);
//            return StatusCode(500, new { message = "Erro interno do servidor" });
//        }
//    }

//    [HttpPut("users/{userId}/deactivate")]
//    public async Task<ActionResult> DeactivateUser(long userId)
//    {
//        try
//        {
//            var currentUserId = GetCurrentUserId();
//            if (currentUserId == userId)
//            {
//                return BadRequest(new { message = "Você não pode desativar sua própria conta" });
//            }

//            var user = await _userService.GetByIdAsync(userId);
//            if (user == null)
//            {
//                return NotFound(new { message = "Usuário não encontrado" });
//            }

//            // Verificar se é o último admin
//            if (user.Role == "ADMIN")
//            {
//                var allUsers = await _userService.GetAllAsync();
//                var activeAdminCount = allUsers.Count(u => u.Role == "ADMIN" && u.IsActive == "Y" && u.Id != userId);

//                if (activeAdminCount == 0)
//                {
//                    return BadRequest(new { message = "Não é possível desativar o último administrador ativo" });
//                }
//            }

//            user.IsActive = "N";
//            await _userService.UpdateAsync(user);

//            _logger.LogInformation("Usuário {UserId} desativado pelo admin {AdminId}", userId, currentUserId);

//            return Ok(new { message = "Usuário desativado com sucesso" });
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Erro ao desativar usuário {UserId}", userId);
//            return StatusCode(500, new { message = "Erro interno do servidor" });
//        }
//    }

//    [HttpPut("users/{userId}/role")]
//    public async Task<ActionResult> ChangeUserRole(long userId, [FromBody] ChangeRoleDTO changeRoleDto)
//    {
//        try
//        {
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(ModelState);
//            }

//            var currentUserId = GetCurrentUserId();
//            if (currentUserId == userId && changeRoleDto.NewRole != "ADMIN")
//            {
//                return BadRequest(new { message = "Você não pode remover seu próprio privilégio de administrador" });
//            }

//            var user = await _userService.GetByIdAsync(userId);
//            if (user == null)
//            {
//                return NotFound(new { message = "Usuário não encontrado" });
//            }

//            // Verificar se é o último admin e está tentando mudar o role
//            if (user.Role == "ADMIN" && changeRoleDto.NewRole != "ADMIN")
//            {
//                var allUsers = await _userService.GetAllAsync();
//                var adminCount = allUsers.Count(u => u.Role == "ADMIN" && u.IsActive == "Y" && u.Id != userId);

//                if (adminCount == 0)
//                {
//                    return BadRequest(new { message = "Não é possível alterar o role do último administrador" });
//                }
//            }

//            var validRoles = new[] { "DONOR", "NGO_MEMBER", "ADMIN" };
//            if (!validRoles.Contains(changeRoleDto.NewRole))
//            {
//                return BadRequest(new { message = "Role inválido" });
//            }

//            user.Role = changeRoleDto.NewRole;
//            await _userService.UpdateAsync(user);

//            _logger.LogInformation("Role do usuário {UserId} alterado para {NewRole} pelo admin {AdminId}",
//                userId, changeRoleDto.NewRole, currentUserId);

//            return Ok(new { message = "Role alterado com sucesso" });
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Erro ao alterar role do usuário {UserId}", userId);
//            return StatusCode(500, new { message = "Erro interno do servidor" });
//        }
//    }

//    [HttpGet("stats")]
//    public async Task<ActionResult<object>> GetUserStats()
//    {
//        try
//        {
//            var users = await _userService.GetAllAsync();

//            var stats = new
//            {
//                totalUsers = users.Count(),
//                activeUsers = users.Count(u => u.IsActive == "Y"),
//                inactiveUsers = users.Count(u => u.IsActive == "N"),
//                usersByRole = users.GroupBy(u => u.Role)
//                    .ToDictionary(g => g.Key, g => g.Count()),
//                recentRegistrations = users.Where(u => u.CreatedAt >= DateTime.Now.AddDays(-30))
//                    .Count(),
//                usersWithOrganization = users.Count(u => u.OrganizationId.HasValue)
//            };

//            return Ok(stats);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Erro ao obter estatísticas de usuários");
//            return StatusCode(500, new { message = "Erro interno do servidor" });
//        }
//    }

//    private long GetCurrentUserId()
//    {
//        var userIdClaim = User.FindFirst("user_id")?.Value;
//        if (long.TryParse(userIdClaim, out long userId))
//        {
//            return userId;
//        }
//        return 0;
//    }
//}

//public class ChangeRoleDTO
//{
//    [Required(ErrorMessage = "Novo role é obrigatório")]
//    public string NewRole { get; set; }
//}