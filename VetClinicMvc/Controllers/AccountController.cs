using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetClinicMvc.Data;
using VetClinicMvc.Models;
using VetClinicMvc.ViewModels;



namespace VetClinicMvc.Controllers;
public class AccountController(VetDbContext db) : Controller
{
    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var email = model.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email))
        {
            ModelState.AddModelError(nameof(model.Email), "Email is already registered.");
            return View(model);
        }
        var user = new User { FullName = model.FullName.Trim(), Email = email, Role = "owner" };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, model.Password);
        db.Users.Add(user);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(model.Email), "Could not create account. Check if the email is already in use.");
            return View(model);
        }
        TempData["Message"] = "Account created. Login will be added in a later Jira story.";
        return RedirectToAction("Index", "Home");
    }
}
