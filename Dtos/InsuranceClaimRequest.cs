namespace CarInsurance.Api.Dtos;

public class InsuranceClaimRequest
{
    public string Date { get; set; }
    public string Description { get; set; }
    public int Amount { get; set; }
}