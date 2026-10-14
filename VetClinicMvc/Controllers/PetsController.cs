using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetClinicMvc.Data;
using VetClinicMvc.Models;
using VetClinicMvc.ViewModels;

namespace VetClinicMvc.Controllers;

[Authorize]
public class PetsController : Controller
{
    private readonly VetDbContext _db;

    public PetsController(VetDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Create() => View(new CreatePetViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePetViewModel model)
    {
        if (model.BirthDate.HasValue && model.BirthDate.Value.Date > DateTime.Today)
            ModelState.AddModelError(nameof(model.BirthDate), "Date of birth cannot be in the future.");

        if (!ModelState.IsValid) return View(model);

        // IMPORTANT: Your login must create ClaimTypes.NameIdentifier with the database User.Id.
        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdText, out var ownerId))
            return Forbid();

        // ADAPT THESE PROPERTY NAMES to your existing Models/Pet.cs.
        var pet = new Pet
        {
            Name = model.Name.Trim(),
            Species = model.Species.Trim(),
            Breed = model.Breed?.Trim(),
            BirthDate = model.BirthDate,
            OwnerId = ownerId
        };

        // ADAPT DbSet name if your VetDbContext uses something other than Pets.
        _db.Pets.Add(pet);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Pet added successfully.";
        return RedirectToAction(nameof(Create));
    }
}
