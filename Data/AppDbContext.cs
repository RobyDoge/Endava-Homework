using CarInsurance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<InsurancePolicy> Policies => Set<InsurancePolicy>();
    public DbSet<InsuranceClaim> Claims => Set<InsuranceClaim>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Car>()
            .HasIndex(c => c.Vin)
            .IsUnique();

        modelBuilder.Entity<InsurancePolicy>()
            .Property(p => p.StartDate)
            .IsRequired();

        modelBuilder.Entity<InsurancePolicy>()
            .Property(p => p.EndDate)
            .IsRequired();
    }
}

public static class SeedData
{
    public static void EnsureSeeded(AppDbContext db)
    {
        if (db.Owners.Any()) return;

        var ana = new Owner { Name = "Ana Pop", Email = "ana.pop@example.com" };
        var bogdan = new Owner { Name = "Bogdan Ionescu", Email = "bogdan.ionescu@example.com" };
        var robert = new Owner { Name = "Popa Robert", Email =  "robert.popa@exemple.com" };
        db.Owners.AddRange(ana, bogdan,robert);
        db.SaveChanges();

        var cars = new List<Car>
        {
            new() { Vin = "VIN12345", Make = "Dacia", Model = "Logan", YearOfManufacture = 2018, OwnerId = ana.Id },
            new() { Vin = "VIN67890", Make = "VW", Model = "Golf", YearOfManufacture = 2021, OwnerId = bogdan.Id },
            new() { Vin = "VIN12345", Make = "VW", Model = "Polo", YearOfManufacture = 2025, OwnerId = robert.Id },
        };
        foreach(var car in cars)
        {
            if(CarExists(db,car))
                    car.Vin = "VIN" + Guid.NewGuid().ToString()[..5];
            db.Cars.Add(car);
            db.SaveChanges();
        }

        db.Policies.AddRange(
            new InsurancePolicy { CarId = cars[0].Id, Provider = "Allianz", StartDate = new DateOnly(2024,1,1), EndDate = new DateOnly(2024,12,31) },
            new InsurancePolicy { CarId = cars[0].Id, Provider = "Groupama", StartDate = new DateOnly(2025,1,1), EndDate = new DateOnly(2025, 12,31) },
            new InsurancePolicy { CarId = cars[1].Id, Provider = "Allianz", StartDate = new DateOnly(2025,3,1), EndDate = new DateOnly(2025,9,30) },
            new InsurancePolicy { CarId = cars[2].Id, Provider = "Groupama", StartDate = new DateOnly(2025, 6, 23), EndDate = new DateOnly(2025, 9, 23) }

        );
        db.SaveChanges();
    }

    private static bool CarExists(AppDbContext db, Car newCar)
    {
        var existingCar = db.Cars.FirstOrDefault(c => c.Vin == newCar.Vin);
        return existingCar != null;
    }
}


