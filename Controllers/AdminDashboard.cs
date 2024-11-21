using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Claimed.Enums;
using Claimed.Models;
using Microsoft.EntityFrameworkCore;

namespace Claimed.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboard : Controller
    {
        private readonly ClaimsDbContext _context;
        private readonly IHubContext<ClaimHub> _claimHub;
        private readonly ILogger<AdminDashboard> _logger;

        public AdminDashboard(ClaimsDbContext context, IHubContext<ClaimHub> claimHub, ILogger<AdminDashboard> logger)
        {
            _context = context;
            _claimHub = claimHub;
            _logger = logger;
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var pendingClaims = await _context.Claims
                                                .Where(c => c.Status == ClaimStatus.Pending.ToString())
                                                .ToListAsync();
                // Auto-reject claims with less than 0.5 hours worked
                await AutoRejectClaims(pendingClaims);
                return View(pendingClaims);
            }
            catch (Exception ex)
            {
                // Log the exception and display an error message.
                _logger.LogError(ex, "Error fetching pending claims.");
                ModelState.AddModelError("DashboardError", "An error occurred while loading the dashboard.");
                return View();
            }
        }

        private async Task AutoRejectClaims(List<Claim> claims)
        {
            foreach (var claim in claims)
            {
                if (claim.HoursWorked < 0.5d || claim.HourlyRate > 2500.0m || claim.HourlyRate < 500.0m)
                {
                    claim.Status = ClaimStatus.Rejected.ToString();
                    await _context.SaveChangesAsync();

                    // Broadcast the status change using SignalR
                    await _claimHub.Clients.All.SendAsync("UpdateClaimStatus", claim.ClaimId, claim.Status.ToString());
                }
            }
        }

        public async Task AutoRejectClaimsForTesting(List<Claim> claims)
        {
            await AutoRejectClaims(claims);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveClaim(int claimId)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null)
            {
                return NotFound();
            }

            try
            {
                claim.Status = ClaimStatus.Approved.ToString();
                await _context.SaveChangesAsync();

                // Broadcast the status change using SignalR
                await _claimHub.Clients.All.SendAsync("UpdateClaimStatus", claimId, claim.Status.ToString());

                return RedirectToAction("Dashboard");
            }
            catch (DbUpdateException dex)
            {
                // Handle database update exceptions.
                _logger.LogError(dex, "Error updating claim status.");
                ModelState.AddModelError("ApproveError", "Failed to approve the claim. Please try again.");
                return View("Dashboard");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RejectClaim(int claimId)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim == null)
            {
                return NotFound();
            }

            try
            {
                claim.Status = ClaimStatus.Rejected.ToString();
                await _context.SaveChangesAsync();

                // Broadcast the status change using SignalR
                await _claimHub.Clients.All.SendAsync("UpdateClaimStatus", claimId, claim.Status.ToString());

                return RedirectToAction("Dashboard");
            }
            catch (DbUpdateException dex)
            {
                // Handle database update exceptions.
                _logger.LogError(dex, "Error updating claim status.");
                ModelState.AddModelError("RejectError", "Failed to reject the claim. Please try again.");
                return View("Dashboard");
            }
        }
    }
}