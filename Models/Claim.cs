namespace Claimed.Models
{
    public class Claim
    {
        public int ClaimId { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public string Course { get; set; }
        public string Description { get; set; }
        public decimal HourlyRate { get; set; }
        public double HoursWorked { get; set; }
        public string? SupportingDocument { get; set; }
        public string Status { get; set; } // 'Pending', 'Approved', 'Rejected'
        public DateTime DateSubmitted { get; set; }
        public DateTime? DateReviewed { get; set; } // Nullable, only set when reviewed
    }
}