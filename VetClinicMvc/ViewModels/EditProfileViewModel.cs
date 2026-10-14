using System.ComponentModel.DataAnnotations;

namespace VetClinicMvc.ViewModels;

public class EditProfileViewModel
{
    [Required, StringLength(150)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Phone, StringLength(30)]
    public string? Phone { get; set; }
}
