
using CarInsurance.Api.Controllers;
using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Models;
using CarInsurance.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CarInsurance.Tests;

[TestClass]
public class CarControllerTests
{
    private readonly CarService _service;
    private readonly CarsController _controller;
    private DbContextOptions<AppDbContext> _options;
    private readonly AppDbContext _dbContext;

    public CarControllerTests()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new AppDbContext(_options);
        _service = new CarService(_dbContext);
        _controller = new CarsController(_service);
    }

    [TestInitialize]
    public void Setup()
    {
        var owner = new Owner {Id=1, Name = "Robert", Email = "robert.popa@exemple.com" };
        _dbContext.Owners.AddRange(owner);
        _dbContext.SaveChanges();

        var car = new Car {Id=1, Vin = "VIN", Make = "VW", Model = "Polo", YearOfManufacture = 2025, OwnerId = 1};
        _dbContext.Cars.Add(car);
        _dbContext.SaveChanges();

        var policies = new List<InsurancePolicy>
        {
            new () { CarId = 1, Provider = "Allianz", StartDate = new DateOnly(2024,1,1), EndDate = new DateOnly(2024,12,31) },
            new () { CarId = 1, Provider = "Groupama", StartDate = new DateOnly(2025,1,1), EndDate = new DateOnly(2025, 12,31) },
        };
        _dbContext.Policies.AddRange(policies);
        _dbContext.SaveChanges();
    }

    [TestMethod]
    public async Task IsInsuranceValid_InvalidDateFormat_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.IsInsuranceValid(1, "23-03-2025");

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        Assert.AreEqual("Invalid date format. Use YYYY-MM-DD.", ((BadRequestObjectResult)result.Result).Value!.ToString());
    }

    [TestMethod]
    public async Task IsInsuranceValid_InvalidDate_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.IsInsuranceValid(1, "2025-06-32");

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        Assert.AreEqual("Invalid date format. Use YYYY-MM-DD.", ((BadRequestObjectResult)result.Result).Value!.ToString());
    }

    [TestMethod]
    public async Task IsInsuranceValid_InvalidCarId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.IsInsuranceValid(2, "2025-06-23");

        // Assert
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        Assert.AreEqual("Car 2 not found", ((NotFoundObjectResult)result.Result).Value!.ToString());
    }


    [TestMethod]
    public async Task IsInsuranceValid_MultipleDates_ReturnsOk()
    {
        // Arrange
        var dates = new List<Tuple<string, bool>>
        {
            new("2025-06-23", true),
            new("2025-12-31", true),
            new("2024-12-31", true),
            new("2024-01-01", true),
            new("2023-12-31", false),
            new("2026-01-01", false),
        };

        foreach (var date in dates)
        {
            var result = await _controller.IsInsuranceValid(1, date.Item1);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result.Result;
            var response = (InsuranceValidityResponse)okResult.Value!;
            Assert.AreEqual(date.Item2, response.Valid);
        }
    }

    [TestMethod]
    public async Task RegisterInsuranceClaim_InvalidDateFormat_ReturnsBadRequest()
    {
        var request = new InsuranceClaimRequest
        {
            Date = "23-03-2025", // Invalid
            Description = "Crash damage",
            Amount = 1000
        };

        var result = await _controller.RegisterInsuranceClaim(1, request);

        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        Assert.AreEqual("Invalid date format. Use YYYY-MM-DD.", ((BadRequestObjectResult)result.Result).Value!.ToString());
    }

    [TestMethod]
    public async Task RegisterInsuranceClaim_EmptyDescription_ReturnsBadRequest()
    {
        var request = new InsuranceClaimRequest
        {
            Date = "2025-03-23",
            Description = "",
            Amount = 1000
        };

        var result = await _controller.RegisterInsuranceClaim(1, request);

        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        Assert.AreEqual("Description is required.", ((BadRequestObjectResult)result.Result).Value!.ToString());
    }

    [TestMethod]
    public async Task RegisterInsuranceClaim_NegativeAmount_ReturnsBadRequest()
    {
        var request = new InsuranceClaimRequest
        {
            Date = "2025-03-23",
            Description = "Crash damage",
            Amount = -500
        };

        var result = await _controller.RegisterInsuranceClaim(1, request);

        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        Assert.AreEqual("Amount must be greater than zero.", ((BadRequestObjectResult)result.Result).Value!.ToString());
    }

    [TestMethod]
    public async Task RegisterInsuranceClaim_InvalidCarId_ReturnsNotFound()
    {
        var request = new InsuranceClaimRequest
        {
            Date = "2025-03-23",
            Description = "Crash damage",
            Amount = 1000
        };

        var result = await _controller.RegisterInsuranceClaim(2, request);

        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        Assert.AreEqual("Car 2 not found", ((NotFoundObjectResult)result.Result).Value!.ToString());
    }

    [TestMethod]
    public async Task RegisterInsuranceClaim_ValidRequest_ReturnsOk()
    {
        var request = new InsuranceClaimRequest
        {
            Date = "2025-03-23",
            Description = "Crash damage",
            Amount = 1500
        };

        var result = await _controller.RegisterInsuranceClaim(1, request);

        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        var okResult = (OkObjectResult)result.Result;
        var response = (InsuranceClaimResponse)okResult.Value!;

        Assert.AreEqual(1, response.CarId);
        Assert.AreEqual("2025-03-23", response.Date);
        Assert.AreEqual("Crash damage", response.Description);
        Assert.AreEqual(1500, response.Amount);
        Assert.IsTrue(response.ClaimRegistered);
    }
}
