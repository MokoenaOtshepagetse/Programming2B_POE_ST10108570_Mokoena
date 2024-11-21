namespace Claimed.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public int ClaimId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Total { get; set; }
    }
}