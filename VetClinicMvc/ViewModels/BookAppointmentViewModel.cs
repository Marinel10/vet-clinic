using System;
using System.ComponentModel.DataAnnotations;

namespace VetClinicMvc.ViewModels;

public class BookAppointmentViewModel
{
    [Required(ErrorMessage = "Choose a pet.")]
    [Range(1, int.MaxValue)]
    public int PetId { get; set; }

    [Required(ErrorMessage = "Choose a veterinarian.")]
    [Range(1, int.MaxValue)]
    public int VeterinarianId { get; set; }

    [Required(ErrorMessage = "Choose a date and time.")]
    public DateTime StartsAt { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }
}
