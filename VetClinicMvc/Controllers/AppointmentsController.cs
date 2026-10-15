using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetClinicMvc.Data;
using VetClinicMvc.Models;
using VetClinicMvc.ViewModels;

namespace VetClinicMvc.Controllers;

[Authorize]
public class AppointmentsController : Controller
{
    private readonly VetDbContext _db;
    public AppointmentsController(VetDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!TryGetOwnerId(out var ownerId)) return Forbid();
        await PopulateLists(ownerId);
        return View(new BookAppointmentViewModel { StartsAt = DateTime.Now.AddDays(1) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookAppointmentViewModel model)
    {
        if (!TryGetOwnerId(out var ownerId)) return Forbid();

        if (model.StartsAt <= DateTime.Now)
            ModelState.AddModelError(nameof(model.StartsAt), "Choose a future date and time.");

        bool ownsPet = await _db.Pets.AnyAsync(p => p.Id == model.PetId && p.OwnerId == ownerId);
        if (!ownsPet)
            ModelState.AddModelError(nameof(model.PetId), "Select one of your own pets.");

        bool validVet = await _db.Users.AnyAsync(u => u.Id == model.VeterinarianId && u.Role == "veterinarian");
        if (!validVet)
            ModelState.AddModelError(nameof(model.VeterinarianId), "Select a valid veterinarian.");

        if (ModelState.IsValid && await _db.Appointments.AnyAsync(a =>
                a.VeterinarianId == model.VeterinarianId &&
                a.StartsAt == model.StartsAt && a.Status == "scheduled"))
            ModelState.AddModelError(nameof(model.StartsAt), "This time slot is already booked.");

        if (!ModelState.IsValid)
        {
            await PopulateLists(ownerId);
            return View(model);
        }

        _db.Appointments.Add(new Appointment
        {
            PetId = model.PetId,
            VeterinarianId = model.VeterinarianId,
            StartsAt = model.StartsAt,
            Reason = model.Reason?.Trim(),
            Status = "scheduled"
        });

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // A concurrent request may have reserved the same slot.
            ModelState.AddModelError(nameof(model.StartsAt), "Could not book this slot. Please choose another time.");
            await PopulateLists(ownerId);
            return View(model);
        }

        TempData["Success"] = "Appointment booked successfully.";
        return RedirectToAction(nameof(Create));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (!TryGetOwnerId(out var ownerId)) return Forbid();

        var appointment = await _db.Appointments.AsNoTracking()
            .Join(_db.Pets, a => a.PetId, p => p.Id, (a, p) => new { Appointment = a, Pet = p })
            .Where(x => x.Appointment.Id == id && x.Pet.OwnerId == ownerId)
            .Select(x => x.Appointment)
            .FirstOrDefaultAsync();

        if (appointment == null) return NotFound();
        if (appointment.Status != "scheduled") return BadRequest("Only scheduled appointments can be rescheduled.");

        return View(new RescheduleAppointmentViewModel
        {
            Id = appointment.Id,
            StartsAt = appointment.StartsAt
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RescheduleAppointmentViewModel model)
    {
        if (!TryGetOwnerId(out var ownerId)) return Forbid();
        if (id != model.Id) return BadRequest();

        var appointment = await _db.Appointments
            .Join(_db.Pets, a => a.PetId, p => p.Id, (a, p) => new { Appointment = a, Pet = p })
            .Where(x => x.Appointment.Id == id && x.Pet.OwnerId == ownerId)
            .Select(x => x.Appointment)
            .FirstOrDefaultAsync();

        if (appointment == null) return NotFound();
        if (appointment.Status != "scheduled") return BadRequest("Only scheduled appointments can be rescheduled.");

        if (model.StartsAt <= DateTime.Now)
            ModelState.AddModelError(nameof(model.StartsAt), "Choose a future date and time.");

        if (ModelState.IsValid && await _db.Appointments.AnyAsync(a =>
            a.Id != appointment.Id &&
            a.VeterinarianId == appointment.VeterinarianId &&
            a.StartsAt == model.StartsAt &&
            a.Status == "scheduled"))
        {
            ModelState.AddModelError(nameof(model.StartsAt), "This time slot is already booked.");
        }

        if (!ModelState.IsValid) return View(model);

        appointment.StartsAt = model.StartsAt;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Appointment rescheduled successfully.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    private bool TryGetOwnerId(out int ownerId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out ownerId);

    private async Task PopulateLists(int ownerId)
    {
        var pets = await _db.Pets.AsNoTracking()
            .Where(p => p.OwnerId == ownerId)
            .OrderBy(p => p.Name)
            .Select(p => new { p.Id, p.Name })
            .ToListAsync();
        var vets = await _db.Users.AsNoTracking()
            .Where(u => u.Role == "veterinarian")
            .OrderBy(u => u.FullName)
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync();
        ViewBag.Pets = new SelectList(pets, "Id", "Name");
        ViewBag.Veterinarians = new SelectList(vets, "Id", "FullName");
    }
}
