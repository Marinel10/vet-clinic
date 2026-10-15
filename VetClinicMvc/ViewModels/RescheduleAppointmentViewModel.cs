using System;
using System.ComponentModel.DataAnnotations;

namespace VetClinicMvc.ViewModels;

public class RescheduleAppointmentViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Choose a date and time.")]
    public DateTime StartsAt { get; set; }
}
