using CarInsurance.Api.Data;
using CarInsurance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public class PolicyService(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task<List<InsurancePolicy>> GetExpiredPolicies()
    {
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _db.Policies.Where(p => p.EndDate.Value< now).ToListAsync();
    }
}