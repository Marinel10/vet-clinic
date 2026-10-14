using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetClinicMvc.Data;
using VetClinicMvc.ViewModels;

namespace VetClinicMvc.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly VetDbContext _db;
    public ProfileController(VetDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Forbid();

        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        return View(new EditProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Forbid();

        if (!ModelState.IsValid) return View(model);

        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        var email = model.Email.Trim();
        if (await _db.Users.AnyAsync(u => u.Email == email && u.Id != userId))
        {
            ModelState.AddModelError(nameof(model.Email), "This email is already in use.");
            return View(model);
        }

        user.FullName = model.FullName.Trim();
        user.Email = email;
        user.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
        await _db.SaveChangesAsync();

        // Refresh the cookie so the navigation/user identity shows the updated email.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Email, user.Email)
        };
        // Preserve existing claims (e.g. roles) instead of discarding them.
        foreach (var claim in User.Claims.Where(c =>
                     c.Type != ClaimTypes.NameIdentifier &&
                     c.Type != ClaimTypes.Name &&
                     c.Type != ClaimTypes.Email))
            claims.Add(claim);

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        TempData["Success"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Edit));
    }
}
