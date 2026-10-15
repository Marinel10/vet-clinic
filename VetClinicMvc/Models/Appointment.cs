using System;

namespace VetClinicMvc.Models;

public class Appointment
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public int VeterinarianId { get; set; }
    public DateTime StartsAt { get; set; }
    public string Status { get; set; } = "scheduled";
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}
