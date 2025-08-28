using CarInsurance.Api.Models;
using System.Text.Json.Serialization;

namespace CarInsurance.Api.Dtos;

public record CarDto(long Id, string Vin, string? Make, string? Model, int Year, long OwnerId, string OwnerName, string? OwnerEmail);
public record InsuranceValidityResponse(long CarId, string Date, bool Valid);
public record InsuranceClaimResponse(long CarId, string  Date, string Description, int Amount, bool ClaimRegistered);
public record CarHistory(
    long CarId, 
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    InsuranceClaim? Claim = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    InsurancePolicy? Policy = null
    );