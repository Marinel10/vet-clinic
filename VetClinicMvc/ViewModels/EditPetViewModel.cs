using System;
using System.ComponentModel.DataAnnotations;

namespace VetClinicMvc.ViewModels;

public class EditPetViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Species { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Breed { get; set; }

    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }
}
