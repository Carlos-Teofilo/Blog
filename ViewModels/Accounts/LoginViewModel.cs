using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels.Accounts;

public class LoginViewModel
{
    [Required(ErrorMessage = "Informe o email")]
    [EmailAddress(ErrorMessage = "E-mail invalido")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Digite a senha")]
    public string Password { get; set; }
}