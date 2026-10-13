using System.ComponentModel.DataAnnotations;
namespace VetClinicMvc.ViewModels;
public class RegisterViewModel
{
    [Required, StringLength(150)] public string FullName { get; set; } = "";
    [Required, EmailAddress, StringLength(255)] public string Email { get; set; } = "";
    [Required, MinLength(8), DataType(DataType.Password)] public string Password { get; set; } = "";
    [Required, Compare(nameof(Password)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = "";
}
