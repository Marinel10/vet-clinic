using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetClinicMvc.Data;
using VetClinicMvc.Models;
using VetClinicMvc.ViewModels;

namespace VetClinicMvc.Controllers;

public class LoginController : Controller
{
    private readonly VetDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    public LoginController(VetDbContext db, IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    public IActionResult Index() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Email, user.Email)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}
