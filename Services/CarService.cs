using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public class CarService(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task<List<CarDto>> ListCarsAsync()
    {
        return await _db.Cars.Include(c => c.Owner)
            .Select(c => new CarDto(c.Id, c.Vin, c.Make, c.Model, c.YearOfManufacture,
                                    c.OwnerId, c.Owner.Name, c.Owner.Email))
            .ToListAsync();
    }

    public async Task<bool> IsInsuranceValidAsync(long carId, DateOnly date)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        return await _db.Policies.AnyAsync(p =>
            p.CarId == carId &&
            p.StartDate <= date &&
            (p.EndDate == null || p.EndDate >= date)
        );
    }

    public async Task<bool> RegisterInsuranceClaimAsync(long carId, DateOnly date, string description, int amount)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        var activePolicy = await _db.Policies
            .Where(p => p.CarId == carId && p.StartDate <= date && p.EndDate >= date)
            .OrderByDescending(p => p.StartDate)
            .FirstOrDefaultAsync();
        if (activePolicy == null) throw new InvalidOperationException($"No active insurance policy for car {carId} on {date}");

        var claim = new Models.InsuranceClaim
        {
            CarId = carId,
            PolicyId = activePolicy.Id,
            Date = date,
            Description = description,
            Amount = amount
        };
        _db.Claims.Add(claim);
        await _db.SaveChangesAsync();
        return claim.Id > 0;
    }
}
