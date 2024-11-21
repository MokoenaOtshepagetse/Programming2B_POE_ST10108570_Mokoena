using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

public class ClaimHub : Hub
{
    private readonly ILogger<ClaimHub> _logger;

    public ClaimHub(ILogger<ClaimHub> logger)
    {
        _logger = logger;
    }

    public async Task UpdateClaimStatus(int claimId, string newStatus)
    {
        try
        {
            // Validate newStatus value
            if (!IsValidStatus(newStatus))
            {
                _logger.LogError("Invalid status value: {newStatus}", newStatus);
                return;
            }

            // Broadcast the status change to all connected clients
            await Clients.All.SendAsync("UpdateClaimStatus", claimId, newStatus);
        }
        catch (Exception ex)
        {
            // Log any exceptions that occur during broadcasting.
            _logger.LogError(ex, "Error broadcasting claim status update.");
        }
    }

    private bool IsValidStatus(string status)
    {
        // Define a list of valid statuses or use an enumeration.
        var validStatuses = new List<string> { "Pending", "Approved", "Rejected" };
        return validStatuses.Contains(status);
    }
}