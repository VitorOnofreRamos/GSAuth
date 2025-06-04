using System.ComponentModel.DataAnnotations;

namespace GSAuth.DTOs;

public class UserDTO
{
    public long Id { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
    public string IsActive { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? OrganizationId { get; set; }
}

public class RegisterDTO
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(255, ErrorMessage = "Nome deve ter no máximo 255 caracteres")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Senha é obrigatória")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres")]
    public string Password { get; set; }

    [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
    public string Phone { get; set; }

    [Required(ErrorMessage = "Role é obrigatório")]
    public string Role { get; set; } // DONOR, NGO_MEMBER, ADMIN

    public long? OrganizationId { get; set; }
}

public class LoginDTO
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Senha é obrigatória")]
    public string Password { get; set; }
}

public class UpdateUserDTO
{
    [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
    [StringLength(255, ErrorMessage = "Email deve ter no máximo 255 caracteres")]
    public string Email { get; set; }

    [StringLength(255, ErrorMessage = "Nome deve ter no máximo 255 caracteres")]
    public string Name { get; set; }

    [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres")]
    public string Password { get; set; }

    [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
    public string Phone { get; set; }
    public string Role { get; set; }
    public bool IsActive { get; set; }
    public int? OrganizationId { get; set; }
}

public class AuthResponseDTO
{
    public string Token { get; set; }
    public UserDTO User { get; set; }
}

public class ChangePasswordDTO
{
    [Required(ErrorMessage = "Senha atual é obrigatória")]
    public string CurrentPassword { get; set; }

    [Required(ErrorMessage = "Nova senha é obrigatória")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Nova senha deve ter entre 6 e 100 caracteres")]
    public string NewPassword { get; set; }
}