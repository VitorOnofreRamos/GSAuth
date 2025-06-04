using GSAuth.DTOs;
using GSAuth.Models;
using GSAuth.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GSAuth.Services;

public class AuthService : _Service, IAuthService
{
    private readonly IUserService _userService;
    private readonly string _jwtSecret;
    private readonly int _jwtExpirationMinutes;

    public AuthService(
        _IRepository<User> userRepository,
        IUserService userService,
        string jwtSecret,
        int jwtExpirationMinutes) : base(userRepository)
    {
        _userService = userService;
        _jwtSecret = jwtSecret;
        _jwtExpirationMinutes = jwtExpirationMinutes;
    }

    public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDto)
    {
        try
        {
            // Verificar se o email já está em uso
            var existingUser = await _userService.GetByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email já está em uso");
            }

            // Validar role
            if (!IsValidRole(registerDto.Role))
            {
                throw new ArgumentException("Role inválido");
            }

            // Criar hash da senha
            var passwordHash = CreatePasswordHash(registerDto.Password);

            // Criar novo usuário
            var user = new User
            {
                Email = registerDto.Email.ToLower().Trim(),
                Name = registerDto.Name.Trim(),
                Phone = registerDto.Phone,
                PasswordHash = passwordHash,
                Role = registerDto.Role,
                IsActive = "Y",
                CreatedAt = DateTime.Now,
                OrganizationId = registerDto.OrganizationId
            };

            await _userService.CreateAsync(user);

            // Gerar token JWT
            var token = GenerateJwtToken(user);

            return new AuthResponseDTO
            {
                Token = token,
                User = MapToUserDto(user)
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao registrar usuário: {ex.Message}", ex);
        }
    }

    public async Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto)
    {
        try
        {
            var user = await _userService.GetByEmailAsync(loginDto.Email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Credenciais inválidas");
            }

            if (user.IsActive != "Y")
            {
                throw new UnauthorizedAccessException("Usuário inativo");
            }

            if (!VerifyPasswordHash(loginDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Credenciais inválidas");
            }

            // Atualizar LastLogin
            user.LastLogin = DateTime.Now;
            await _userService.UpdateAsync(user);

            // Gerar token JWT
            var token = GenerateJwtToken(user);

            return new AuthResponseDTO
            {
                Token = token,
                User = MapToUserDto(user)
            };
        }
        catch (Exception ex) when (!(ex is UnauthorizedAccessException))
        {
            throw new InvalidOperationException($"Erro ao fazer login: {ex.Message}", ex);
        }
    }

    public async Task<bool> ChangePasswordAsync(long userId, ChangePasswordDTO changePasswordDto)
    {
        try
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (!VerifyPasswordHash(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Senha atual incorreta");
            }

            // Criar hash da nova senha
            user.PasswordHash = CreatePasswordHash(changePasswordDto.NewPassword);
            user.UpdatedAt = DateTime.Now;

            await _userService.UpdateAsync(user);
            return true;
        }
        catch (Exception ex) when (!(ex is UnauthorizedAccessException))
        {
            throw new InvalidOperationException($"Erro ao alterar senha: {ex.Message}", ex);
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = "GSAuth.API",
                ValidateAudience = true,
                ValidAudience = "GSClients",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<UserDTO> GetCurrentUserAsync(long userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        return user != null ? MapToUserDto(user) : null;
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("user_id", user.Id.ToString()),
            new Claim("is_active", user.IsActive)
        };

        if (user.OrganizationId.HasValue)
        {
            claims.Add(new Claim("organization_id", user.OrganizationId.Value.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes),
            Issuer = "GSAuth.API",
            Audience = "GSClients",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string CreatePasswordHash(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            // Adicionar um salt fixo ou usar um salt único por usuário
            var saltedPassword = password + "GSAuth_Salt_2025"; // Considere usar um salt único por usuário
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private bool VerifyPasswordHash(string password, string storedHash)
    {
        var hashedPassword = CreatePasswordHash(password);
        return hashedPassword == storedHash;
    }

    private bool IsValidRole(string role)
    {
        var validRoles = new[] { "DONOR", "NGO_MEMBER", "ADMIN" };
        return validRoles.Contains(role?.ToUpper());
    }

    private UserDTO MapToUserDto(User user)
    {
        return new UserDTO
        {
            Id = user.Id,
            Email = user.Email,
            Phone = user.Phone,
            Name = user.Name,
            Role = user.Role,
            IsActive = user.IsActive,
            LastLogin = user.LastLogin,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            OrganizationId = user.OrganizationId
        };
    }
}
