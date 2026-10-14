using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VetClinicMvc.Data;
using VetClinicMvc.Models;
using VetClinicMvc.ViewModels;
using Microsoft.EntityFrameworkCore;


namespace VetClinicMvc.Controllers;

[Authorize]
public class PetsController : Controller
{
    private readonly VetDbContext _db;

    public PetsController(VetDbContext db) => _db = db;


    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdText, out var ownerId))
            return Forbid();

        var pets = await _db.Pets
            .AsNoTracking()
            .Where(p => p.OwnerId == ownerId)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return View(pets);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdText, out var ownerId))
            return Forbid();

        var pet = await _db.Pets
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Id == id && p.OwnerId == ownerId);

        if (pet == null)
            return NotFound();

        return View(pet);
    }



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


    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var ownerId))
            return Forbid();

        var pet = await _db.Pets.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == ownerId);
        if (pet is null) return NotFound();

        return View(new EditPetViewModel
        {
            Id = pet.Id,
            Name = pet.Name,
            Species = pet.Species,
            Breed = pet.Breed,
            BirthDate = pet.BirthDate
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditPetViewModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var ownerId))
            return Forbid();

        var pet = await _db.Pets
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == ownerId);
        if (pet is null) return NotFound();

        if (!ModelState.IsValid) return View(model);

        pet.Name = model.Name.Trim();
        pet.Species = model.Species.Trim();
        pet.Breed = string.IsNullOrWhiteSpace(model.Breed) ? null : model.Breed.Trim();
        pet.BirthDate = model.BirthDate;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = pet.Id });
    }



}
