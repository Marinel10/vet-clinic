using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetClinicMvc.Controllers;
using VetClinicMvc.Data;
using VetClinicMvc.Models;
using Xunit;

namespace VetClinicMvc.Tests;

public class PetProfileAccessTests
{
    private static VetDbContext NewDatabase()
    {
        var options = new DbContextOptionsBuilder<VetDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new VetDbContext(options);
    }

    private static PetsController ControllerFor(VetDbContext db, int ownerId)
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, ownerId.ToString()) },
            "TestAuthentication");
        return new PetsController(db)
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

    private static async Task<Pet> SeedPet(VetDbContext db, int ownerId)
    {
        var pet = new Pet
        {
            OwnerId = ownerId,
            Name = "Buddy",
            Species = "Dog"
        };
        db.Pets.Add(pet);
        await db.SaveChangesAsync();
        return pet;
    }

    [Fact]
    public async Task Details_OwnerCanViewOwnPet()
    {
        using var db = NewDatabase();
        var pet = await SeedPet(db, ownerId: 101);
        var controller = ControllerFor(db, ownerId: 101);

        var result = await controller.Details(pet.Id);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Pet>(view.Model);
        Assert.Equal(pet.Id, model.Id);
    }

    [Fact]
    public async Task Details_OtherOwnerCannotViewPetByChangingUrlId()
    {
        using var db = NewDatabase();
        var pet = await SeedPet(db, ownerId: 202);
        var controller = ControllerFor(db, ownerId: 101);

        var result = await controller.Details(pet.Id);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_UnknownPetReturnsNotFound()
    {
        using var db = NewDatabase();
        var controller = ControllerFor(db, ownerId: 101);

        var result = await controller.Details(999999);

        Assert.IsType<NotFoundResult>(result);
    }
}
