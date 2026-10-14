using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetClinicMvc.Controllers;
using VetClinicMvc.Data;
using VetClinicMvc.Models;
using VetClinicMvc.ViewModels;
using Xunit;

namespace VetClinicMvc.Tests;

public class DuplicateEmailProfileTests
{
    private static VetDbContext NewDatabase() => new(
        new DbContextOptionsBuilder<VetDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static ProfileController ControllerFor(VetDbContext db, int userId)
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) },
            "TestAuthentication");
        return new ProfileController(db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            }
        };
    }

    private static async Task SeedUsers(VetDbContext db)
    {
        db.Users.AddRange(
            new User { Id = 101, FullName = "Owner A", Email = "owner.a@example.com" },
            new User { Id = 202, FullName = "Owner B", Email = "owner.b@example.com" });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Edit_DuplicateEmail_ReturnsFormWithValidationError()
    {
        using var db = NewDatabase();
        await SeedUsers(db);
        var controller = ControllerFor(db, 101);
        var model = new EditProfileViewModel
        {
            FullName = "Owner A Updated",
            Email = "owner.b@example.com"
        };

        var result = await controller.Edit(model);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(model.Email)));
    }

    [Fact]
    public async Task Edit_DuplicateEmail_DoesNotChangeEitherUser()
    {
        using var db = NewDatabase();
        await SeedUsers(db);
        var controller = ControllerFor(db, 101);

        await controller.Edit(new EditProfileViewModel
        {
            FullName = "Changed Name",
            Email = "owner.b@example.com"
        });

        var first = await db.Users.FindAsync(101);
        var second = await db.Users.FindAsync(202);
        Assert.Equal("owner.a@example.com", first!.Email);
        Assert.Equal("Owner A", first.FullName);
        Assert.Equal("owner.b@example.com", second!.Email);
    }

    [Fact]
    public void EditProfileViewModel_InvalidEmail_FailsValidation()
    {
        var model = new EditProfileViewModel
        {
            FullName = "Owner A",
            Email = "not-an-email"
        };
        var errors = new List<ValidationResult>();

        var valid = Validator.TryValidateObject(
            model, new ValidationContext(model), errors, validateAllProperties: true);

        Assert.False(valid);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(model.Email)));
    }
}
