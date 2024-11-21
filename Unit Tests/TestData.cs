using Claimed.Enums;
using Claimed.Models;

public class TestData
{
    public List<Claim> GetTestClaims()
    {
        // Auto-rejected claims
        var claim1 = new Claim
        {
            ClaimId = 1,
            UserId = 1,
            Course = "Course A",
            HoursWorked = 0.4,
            HourlyRate = 1000,
            Status = ClaimStatus.Rejected.ToString(),
            DateSubmitted = DateTime.Now.AddDays(-10)
        };

        var claim2 = new Claim
        {
            ClaimId = 2,
            UserId = 2,
            Course = "Course B",
            HoursWorked = 10,
            HourlyRate = 2600,
            Status = ClaimStatus.Rejected.ToString(),
            DateSubmitted = DateTime.Now.AddDays(-10)
        };

        // Approved claims
        var claim3 = new Claim
        {
            ClaimId = 3,
            UserId = 1,
            Course = "Course C",
            HoursWorked = 10,
            HourlyRate = 1500,
            Status = ClaimStatus.Approved.ToString(),
            DateSubmitted = DateTime.Now.AddDays(-10)
        };

        var claim4 = new Claim
        {
            ClaimId = 4,
            UserId = 3,
            Course = "Course A",
            HoursWorked = 10,
            HourlyRate = 1200,
            Status = ClaimStatus.Approved.ToString(),
            DateSubmitted = DateTime.Now.AddDays(-10)
        };

        var claim5 = new Claim
        {
            ClaimId = 5,
            UserId = 4,
            Course = "Course D",
            HoursWorked = 10,
            HourlyRate = 1800,
            Status = ClaimStatus.Approved.ToString(),
            DateSubmitted = DateTime.Now.AddDays(-10)
        };

        var claim6 = new Claim
        {
            ClaimId = 6,
            UserId = 1,
            Course = "Course E",
            HoursWorked = 10,
            HourlyRate = 2000,
            Status = ClaimStatus.Approved.ToString(),
            DateSubmitted = DateTime.Now.AddDays(-10)
        };

        var claim7 = new Claim
        {
            ClaimId = 7,
            UserId = 5,
            Course = "Course F",
            HoursWorked = 10,
            HourlyRate = 2200,
            Status = ClaimStatus.Approved.ToString(),
            DateSubmitted = DateTime.Now.AddDays(-10)
        };

        var claim8 = new Claim
        {
            ClaimId = 3,
            UserId = 1,  // Associated with user1
            Course = "Data Structures",
            Description = "Claim for completing Data Structures course",
            HourlyRate = 700.00M,
            HoursWorked = 15.0,
            SupportingDocument = "/uploads/Proof3.txt",  // Document provided
            Status = "Approved",
            DateSubmitted = DateTime.Now,
            DateReviewed = DateTime.Now
        };

        var claim9 = new Claim
        {
            ClaimId = 4,
            UserId = 2,  // Associated with user2
            Course = "Algorithms",
            Description = "Claim for completing Algorithms course",
            HourlyRate = 800.00M,
            HoursWorked = 18.0,
            SupportingDocument = "/uploads/Proof4.txt",  // Document provided
            Status = "Approved",
            DateSubmitted = DateTime.Now,
            DateReviewed = DateTime.Now
        };

        var claim10 = new Claim
        {
            ClaimId = 5,
            UserId = 3,  // Associated with user3
            Course = "Computer Networks",
            Description = "Claim for completing Computer Networks course",
            HourlyRate = 900.00M,
            HoursWorked = 20.0,
            SupportingDocument = "/uploads/Proof5.txt",  // Document provided
            Status = "Approved",
            DateSubmitted = DateTime.Now,
            DateReviewed = DateTime.Now
        };

        return new List<Claim>
        {
            claim1,
            claim2,
            claim3,
            claim4,
            claim5,
            claim6,
            claim7,
            claim8,
            claim9,
            claim10,
        };
    }
}