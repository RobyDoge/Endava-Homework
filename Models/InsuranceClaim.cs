using System.ComponentModel.DataAnnotations;

namespace CarInsurance.Api.Models;
public class InsuranceClaim
{
    public long Id { get; set; }
    public long CarId { get; set; }
    public long PolicyId { get; set; }
    public DateOnly Date { get; set; }
    public string Description { get; set; }
    public int Amount { get; set; }


}