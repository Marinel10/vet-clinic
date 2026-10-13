namespace VetClinicMvc.Models;
public class Pet
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    public string Name { get; set; } = "";
    public string Species { get; set; } = "";
    public string? Breed { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Sex { get; set; }
    public DateTime CreatedAt { get; set; }
}
