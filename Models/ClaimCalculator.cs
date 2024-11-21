using Claimed.Models;

public interface IClaimCalculator
{
    decimal CalculatePayment(Claim claim);
    decimal calculateClaimAmount(int hoursWorked, int hourlyRate);
}

public class ClaimCalculator : IClaimCalculator
{
    public decimal CalculatePayment(Claim claim)
    {
        decimal payment = (decimal)claim.HoursWorked * claim.HourlyRate;
        decimal tax = payment * 0.1m; // Assuming a 10% tax rate
        return payment - tax;
    }

    public decimal calculateClaimAmount(int hoursWorked, int hourlyRate)
    {
        decimal payment = (decimal)hoursWorked * hourlyRate;
        decimal tax = payment * 0.1m; // Assuming a 10% tax rate
        return payment - tax;
    }
}