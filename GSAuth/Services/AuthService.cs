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
    private readonly IConfiguration _configuration;

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
            var existingUser = await _userService.GetByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email já está em uso");
            }

            if (!IsValidRole(registerDto.Role))
            {
                throw new ArgumentException("Role inválido");
            }

            var passwordHash = CreatePasswordHash(registerDto.Password);

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

            // Importante: Após inserir, precisamos buscar o usuário para obter o ID gerado
            var createdUser = await _userService.GetByEmailAsync(user.Email);
            if (createdUser == null)
            {
                throw new InvalidOperationException("Erro ao criar usuário");
            }

            var token = GenerateJwtToken(createdUser);

            return new AuthResponseDTO
            {
                Token = token,
                User = MapToUserDto(createdUser)
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

            user.LastLogin = DateTime.Now;
            await _userService.UpdateAsync(user);

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
            var key = Encoding.UTF8.GetBytes(_jwtSecret);

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

    public async Task<bool> DeleteAccountAsync(long userId, string password)
    {
        try
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            // Verificar se a senha está correta
            if (!VerifyPasswordHash(password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Senha incorreta");
            }

            // Verificar se é o último admin (opcional - para evitar lock-out)
            if (user.Role == "ADMIN")
            {
                var allUsers = await _userService.GetAllAsync();
                var adminCount = allUsers.Count(u => u.Role == "ADMIN" && u.IsActive == "Y" && u.Id != userId);

                if (adminCount == 0)
                {
                    throw new InvalidOperationException("Não é possível deletar o último administrador do sistema");
                }
            }

            await _userService.DeleteAsync(userId);
            return true;
        }
        catch (Exception ex) when (!(ex is UnauthorizedAccessException || ex is InvalidOperationException))
        {
            throw new InvalidOperationException($"Erro ao deletar conta: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteUserByAdminAsync(long userId)
    {
        try
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            // Verificar se é o último admin
            if (user.Role == "ADMIN")
            {
                var allUsers = await _userService.GetAllAsync();
                var adminCount = allUsers.Count(u => u.Role == "ADMIN" && u.IsActive == "Y" && u.Id != userId);

                if (adminCount == 0)
                {
                    throw new InvalidOperationException("Não é possível deletar o último administrador do sistema");
                }
            }

            await _userService.DeleteAsync(userId);
            return true;
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            throw new InvalidOperationException($"Erro ao deletar usuário: {ex.Message}", ex);
        }
    }

    private string GenerateJwtToken(User user)
    {
        if (user.Id == 0)
        {
            throw new ArgumentException("Usuário deve ter um ID válido para gerar token");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSecret);

        // Claims padronizadas - IMPORTANTE: usar nomes corretos
        var claims = new List<Claim>
        {
            new Claim("sub", user.Id.ToString()),
            new Claim("email", user.Email),
            new Claim("name", user.Name),
            new Claim("role", user.Role),
            new Claim("user_id", user.Id.ToString()), // Claim personalizada para facilitar acesso
            new Claim("is_active", user.IsActive),
            new Claim("jti", Guid.NewGuid().ToString()),
            new Claim("iat", new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString())
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
        var tokenString = tokenHandler.WriteToken(token);

        // Log para debug
        Console.WriteLine($"Token gerado para usuário ID: {user.Id}, Email: {user.Email}");
        Console.WriteLine($"Token (primeiros 50 chars): {tokenString.Substring(0, Math.Min(tokenString.Length, 50))}...");

        return tokenString;
    }

    private string CreatePasswordHash(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var saltedPassword = password + "GSAuth_Salt_2025";
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