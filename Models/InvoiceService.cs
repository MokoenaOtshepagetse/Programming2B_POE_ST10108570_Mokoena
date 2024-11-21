using Claimed.Models;

public class InvoiceService
{
    private readonly ClaimsDbContext _context;

    public InvoiceService(ClaimsDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice> GenerateInvoice(Claim claim)
    {
        // Calculate total amount due
        decimal totalAmount = (decimal)claim.HoursWorked * claim.HourlyRate;

        // Generate invoice number and date
        string invoiceNumber = GenerateInvoiceNumber();
        DateTime invoiceDate = DateTime.Now;

        // Create new invoice instance
        Invoice invoice = new Invoice
        {
            ClaimId = claim.ClaimId,
            InvoiceDate = invoiceDate,
            InvoiceNumber = invoiceNumber,
            TotalAmount = totalAmount,
        };

        // Save invoice to database
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return invoice;
    }

    private string GenerateInvoiceNumber()
    {
        // Implement a unique invoice number generation logic
        return Guid.NewGuid().ToString();
    }
}