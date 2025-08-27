using CarInsurance.Api.Dtos;
using CarInsurance.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarInsurance.Api.Controllers;

[ApiController]
[Route("api")]
public class CarsController(CarService service) : ControllerBase
{
    private readonly CarService _service = service;

    [HttpGet("cars")]
    public async Task<ActionResult<List<CarDto>>> GetCars()
        => Ok(await _service.ListCarsAsync());

    [HttpGet("cars/{carId:long}/insurance-valid")]
    public async Task<ActionResult<InsuranceValidityResponse>> IsInsuranceValid(long carId, [FromQuery] string date)
    {
        if (!DateOnly.TryParse(date, out var parsed))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");

        try
        {
            var valid = await _service.IsInsuranceValidAsync(carId, parsed);
            return Ok(new InsuranceValidityResponse(carId, parsed.ToString("yyyy-MM-dd"), valid));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("/cars/{carId:long}/claims")]
    public async Task<ActionResult<InsuranceClaimResponse>> RegisterInsuranceClaim(long carId, [FromBody] InsuranceClaimRequest request)
    {
        if (!DateOnly.TryParse(request.Date, out var parsed))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");
        if (string.IsNullOrWhiteSpace(request.Description))
            return BadRequest("Description is required.");
        if (request.Amount <= 0)
            return BadRequest("Amount must be greater than zero.");
        try
        {
            var registered = await _service.RegisterInsuranceClaimAsync(carId, parsed, request.Description, request.Amount);
            return Ok(new InsuranceClaimResponse(carId, parsed.ToString("yyyy-MM-dd"), request.Description, request.Amount, registered));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

}
