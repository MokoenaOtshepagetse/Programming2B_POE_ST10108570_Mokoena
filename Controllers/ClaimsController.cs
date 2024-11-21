using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Claimed.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

namespace Claimed.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly ClaimsDbContext _context;
        private readonly IHubContext<ClaimHub> _claimHub;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<AdminDashboard> _logger;

        public ClaimsController(ClaimsDbContext context, IHubContext<ClaimHub> claimHub, IWebHostEnvironment hostingEnvironment, ILogger<AdminDashboard> logger)
        {
            _context = context;
            _claimHub = claimHub;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        public IActionResult ViewClaim(int claimId)
        {
            // Retrieve the claim using claimId
            var claim = _context.Claims.Include(c => c.User).FirstOrDefault(c => c.ClaimId == claimId);

            if (claim == null)
            {
                return NotFound();
            }

            // Return the view with the claim data
            return View(claim);
        }

        public IActionResult EditClaim(int claimId)
        {
            // Retrieve the claim using claimId
            var claim = _context.Claims.Include(c => c.User).FirstOrDefault(c => c.ClaimId == claimId);

            if (claim == null)
            {
                return NotFound();
            }

            // Return the view with the claim data for editing
            return View(claim);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateClaimStatus(int claimId, string newStatus)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null)
            {
                return NotFound();
            }

            try
            {
                claim.Status = newStatus;
                await _context.SaveChangesAsync();

                // Broadcast the status change using SignalR
                await _claimHub.Clients.All.SendAsync("UpdateClaimStatus", claimId, newStatus);

                return Ok();
            }
            catch (DbUpdateException dex)
            {
                // Handle database update exceptions.
                _logger.LogError(dex, "Error updating claim status.");
                ModelState.AddModelError("UpdateError", "Failed to update claim status. Please try again.");
                return View("Dashboard"); // Redirect to an appropriate view for error display.
            }
        }

        public IActionResult SubmitClaim()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateClaim(Claim model, IFormFile supportingDocument)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var claim = new Claim
                {
                    UserId = model.UserId,
                    Course = model.Course,
                    Description = model.Description,
                    HourlyRate = model.HourlyRate,
                    HoursWorked = model.HoursWorked,
                    DateSubmitted = DateTime.Now
                };

                if (supportingDocument != null)
                {
                    var allowedExtensions = new[] { ".txt", ".odt", ".doc", ".docx", ".pdf" };
                    var fileExtension = Path.GetExtension(supportingDocument.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("SupportingDocument", "Invalid file type.");
                        return View(model);
                    }

                    string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                    EnsureDirectoryExists(uploadsFolder);

                    string uniqueFileName = GenerateUniqueFileName(supportingDocument.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await supportingDocument.CopyToAsync(fileStream);
                    }

                    claim.SupportingDocument = uniqueFileName;
                }

                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

                return RedirectToAction("UserDashboard");
            }
            catch (Exception ex)
            {
                // Log the exception and display an error message.
                _logger.LogError(ex, "Error creating a new claim.");
                ModelState.AddModelError("CreateError", "Failed to create the claim. Please try again.");
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveClaim(int claimId)
        {
            // Retrieve the claim using claimId
            var claim = await _context.Claims.FindAsync(claimId);

            if (claim == null)
            {
                return NotFound();
            }
            
            claim.Status = "Approved";
            claim.DateReviewed = DateTime.Now;

            // Generate invoice
            InvoiceService invoiceService = new InvoiceService(_context);
            Invoice invoice = await invoiceService.GenerateInvoice(claim);
            // Broadcast the status change using SignalR
            await _claimHub.Clients.All.SendAsync("UpdateClaimStatus", claimId, claim.Status);

            _context.Claims.Update(claim);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Redirect to a success page or any other appropriate action
        }

        [HttpPost]
        public async Task<IActionResult> RejectClaim(int claimId)
        {
            // Retrieve the claim using claimId
            var claim = await _context.Claims.FindAsync(claimId);

            if (claim == null)
            {
                return NotFound();
            }

            // Implement rejection logic here
            // For example, update the status and perform other necessary actions
            claim.Status = "Rejected";
            claim.DateReviewed = DateTime.Now;

            _context.Claims.Update(claim);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index)); // Redirect to a success page or any other appropriate action
        }

        [HttpPost]
        public async Task<IActionResult> CreateClaim(Claim claim)
        {
            // Validate form data
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateClaim", claim);
            }

            // Save claim to database
            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        private void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private string GenerateUniqueFileName(string fileName)
        {
            string uniqueFileName = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            int count = 0;

            while (System.IO.File.Exists(Path.Combine(_hostingEnvironment.WebRootPath, "uploads", uniqueFileName + extension)))
            {
                uniqueFileName = $"{uniqueFileName}_{++count}{extension}";
            }

            return uniqueFileName;
        }

        public async Task<IActionResult> Index()
        {
            var claims = await _context.Claims
                .Include(c => c.User)
                .OrderBy(c => c.SupportingDocument != null)
                .ThenByDescending(c => c.DateSubmitted)
                .ThenByDescending(c => c.DateReviewed.HasValue)
                .ThenByDescending(c => c.DateReviewed)
                .ToListAsync();

            return View(claims);
        }

        [HttpGet]
        public async Task<IActionResult> PrintInvoice(int id)
        {
            Invoice invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            // Generate PDF
            var pdf = new PdfDocument();
            var page = pdf.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            // Add invoice details to PDF
            var font = new XFont("Arial", 12);
            gfx.DrawString("Invoice Number: " + invoice.InvoiceNumber, font, XBrushes.Black, new XPoint(10, 10));
            gfx.DrawString("Invoice Date: " + invoice.InvoiceDate.ToString("yyyy-MM-dd"), font, XBrushes.Black, new XPoint(10, 30));
            gfx.DrawString("Claim ID: " + invoice.ClaimId, font, XBrushes.Black, new XPoint(10, 50));
            gfx.DrawString("Total Amount: " + invoice.TotalAmount.ToString("C"), font, XBrushes.Black, new XPoint(10, 70));

            // Return PDF as response
            var stream = new MemoryStream();
            pdf.Save(stream);
            stream.Position = 0;
            return File(stream, "application/pdf", "invoice.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> GenerateReport(string userId = null, string courseId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var claims = await _context.Claims.ToListAsync();

            if (int.TryParse(userId, out int parsedUserId))
            {
                claims = claims.Where(c => c.UserId == parsedUserId).ToList();
            }

            if (!string.IsNullOrEmpty(courseId))
            {
                claims = claims.Where(c => c.Course == courseId).ToList();
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                claims = claims.Where(c => c.DateSubmitted >= startDate && c.DateSubmitted <= endDate).ToList();
            }

            // Generate PDF
            var pdf = new PdfDocument();
            var page = pdf.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            // Add report details to PDF
            var font = new XFont("Arial", 12);
            gfx.DrawString("Report", font, XBrushes.Black, new XPoint(10, 10));

            if (!string.IsNullOrEmpty(userId))
            {
                gfx.DrawString("User: " + userId, font, XBrushes.Black, new XPoint(10, 30));
            }

            if (!string.IsNullOrEmpty(courseId))
            {
                gfx.DrawString("Course: " + courseId, font, XBrushes.Black, new XPoint(10, 50));
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                gfx.DrawString("Date Range: " + startDate.Value.ToString("yyyy-MM-dd") + " - " + endDate.Value.ToString("yyyy-MM-dd"), font, XBrushes.Black, new XPoint(10, 70));
            }

            // Add claims to PDF
            int y = 100;
            foreach (var claim in claims)
            {
                gfx.DrawString("Claim ID: " + claim.ClaimId, font, XBrushes.Black, new XPoint(10, y));
                gfx.DrawString("Course: " + claim.Course, font, XBrushes.Black, new XPoint(10, y + 20));
                gfx.DrawString("Description: " + claim.Description, font, XBrushes.Black, new XPoint(10, y + 40));
                gfx.DrawString("Date Submitted: " + claim.DateSubmitted.ToString("yyyy-MM-dd"), font, XBrushes.Black, new XPoint(10, y + 60));
                y += 80;
            }

            // Return PDF as response
            var stream = new MemoryStream();
            pdf.Save(stream);
            stream.Position = 0;
            return File(stream, "application/pdf", "report.pdf");
        }
    }
}