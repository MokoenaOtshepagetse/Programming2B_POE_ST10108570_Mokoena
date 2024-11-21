using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Claimed.Models;
using System;
using Claimed.Controllers;
using Moq;
using Microsoft.AspNetCore.SignalR;

public class AdminControllerTests
{
    private AdminDashboard _controller;
    private TestData _testData;
    private Mock<ClaimsDbContext> _mockContext;
    private Mock<IHubContext<ClaimHub>> _mockClaimHub;
    private Mock<ILogger<AdminDashboard>> _mockLogger;

    public AdminControllerTests()
    {
        _testData = new TestData();
        _mockContext = new Mock<ClaimsDbContext>();
        _mockClaimHub = new Mock<IHubContext<ClaimHub>>();
        _mockLogger = new Mock<ILogger<AdminDashboard>>();

        _controller = new AdminDashboard(
            _mockContext.Object,
            _mockClaimHub.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Dashboard_ReturnsViewResult_WithPendingClaims()
    {
        // Arrange
        var pendingClaims = new List<Claim> { new Claim { ClaimId = 1, Status = "Pending" } };
        var mockContext = new Mock<ClaimsDbContext>();
        mockContext.Setup(x => x.Claims
            .Include(c => c.User)
            .Where(c => c.Status == "Pending")
            .ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingClaims);

        var controller = new AdminDashboard(mockContext.Object, new Mock<IHubContext<ClaimHub>>().Object, new Mock<ILogger<AdminDashboard>>().Object);

        // Act
        var result = await controller.Dashboard() as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ViewResult>(result);
        Assert.Equal(pendingClaims.Count, ((List<Claim>)result.Model).Count);
    }

    [Fact]
    public async Task Dashboard_ReturnsViewResult_WithNoPendingClaims()
    {
        // Arrange
        var pendingClaims = new List<Claim>();
        var mockContext = new Mock<ClaimsDbContext>();
        mockContext.Setup(x => x.Claims
            .Include(c => c.User)
            .Where(c => c.Status == "Pending")
            .ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingClaims);

        var controller = new AdminDashboard(mockContext.Object, new Mock<IHubContext<ClaimHub>>().Object, new Mock<ILogger<AdminDashboard>>().Object);

        // Act
        var result = await controller.Dashboard() as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ViewResult>(result);
        Assert.Empty((List<Claim>)result.Model);
    }

    [Fact]
    public async Task AutoRejectClaims_RejectsClaimsAndSendsNotification()
    {
        // Arrange
        var claimsToReject = new List<Claim> { new Claim { ClaimId = 1, Status = "Pending" } };
        var mockContext = new Mock<ClaimsDbContext>();
        mockContext.Setup(x => x.Claims
            .Include(c => c.User)
            .Where(c => c.Status == "Pending")
            .ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(claimsToReject);

        var mockHub = new Mock<IHubContext<ClaimHub>>();
        mockHub.Setup(x => x.Clients.All.SendAsync("UpdateClaimStatus", It.IsAny<int>(), "Rejected", It.IsAny<CancellationToken>())).Verifiable();

        var controller = new AdminDashboard(mockContext.Object, mockHub.Object, new Mock<ILogger<AdminDashboard>>().Object);

        // Act
        await controller.AutoRejectClaimsForTesting(claimsToReject);

        // Assert
        mockHub.Verify(x => x.Clients.All.SendAsync("UpdateClaimStatus", It.IsAny<int>(), "Rejected", It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("Rejected", claimsToReject.First().Status);
    }

    [Fact]
    public async Task ApproveClaim_ApprovesClaimAndSendsNotification()
    {
        // Arrange
        var claimId = 1;
        var claim = new Claim { ClaimId = claimId, Status = "Pending" };
        var mockContext = new Mock<ClaimsDbContext>();
        mockContext.Setup(x => x.Claims.FindAsync(It.IsAny<int>())).ReturnsAsync(claim);

        var mockHub = new Mock<IHubContext<ClaimHub>>();
        mockHub.Setup(x => x.Clients.All.SendAsync("UpdateClaimStatus", claimId, "Approved", It.IsAny<CancellationToken>())).Verifiable();

        var controller = new AdminDashboard(mockContext.Object, mockHub.Object, new Mock<ILogger<AdminDashboard>>().Object);

        // Act
        await controller.ApproveClaim(claimId);

        // Assert
        mockHub.Verify(x => x.Clients.All.SendAsync("UpdateClaimStatus", claimId, "Approved", It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("Approved", claim.Status);
    }

    [Fact]
    public async Task RejectClaim_RejectsClaimAndSendsNotification()
    {
        // Arrange
        var claimId = 1;
        var claim = new Claim { ClaimId = claimId, Status = "Pending" };
        var mockContext = new Mock<ClaimsDbContext>();
        mockContext.Setup(x => x.Claims.FindAsync(It.IsAny<int>())).ReturnsAsync(claim);

        var mockHub = new Mock<IHubContext<ClaimHub>>();
        mockHub.Setup(x => x.Clients.All.SendAsync("UpdateClaimStatus", claimId, "Rejected", It.IsAny<CancellationToken>())).Verifiable();

        var controller = new AdminDashboard(mockContext.Object, mockHub.Object, new Mock<ILogger<AdminDashboard>>().Object);

        // Act
        await controller.RejectClaim(claimId);

        // Assert
        mockHub.Verify(x => x.Clients.All.SendAsync("UpdateClaimStatus", claimId, "Rejected", It.IsAny<CancellationToken>()), Times.Once);
    }
   
    [Fact]
    public async Task AutoRejectClaims_RejectsClaimsWithHighHourlyRateAndSendsNotification()
    {
        // Arrange
        var claimsToReject = new List<Claim> { new Claim { ClaimId = 1, Status = "Pending", HoursWorked = 1.0d, HourlyRate = 2500.0m } };
        _mockContext.Setup(x => x.Claims
            .Include(c => c.User)
            .Where(c => c.Status == "Pending")
            .ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(claimsToReject);

        _mockClaimHub.Setup(x => x.Clients.All.SendAsync("UpdateClaimStatus", It.IsAny<int>(), "Rejected", It.IsAny<CancellationToken>())).Verifiable();

        var controller = new AdminDashboard(_mockContext.Object, _mockClaimHub.Object, new Mock<ILogger<AdminDashboard>>().Object);

        // Act
        await controller.AutoRejectClaimsForTesting(claimsToReject);

        // Assert
        _mockClaimHub.Verify(x => x.Clients.All.SendAsync("UpdateClaimStatus", It.IsAny<int>(), "Rejected", It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("Rejected", claimsToReject.First().Status);
    }

    [Fact]
        public async Task AutoRejectClaims_DoesNotRejectClaimsThatDoNotMeetCriteria()
        {
            // Arrange
            var claimsToReject = new List<Claim> { new Claim { ClaimId = 1, Status = "Pending", HoursWorked = 1.0d, HourlyRate = 1000.0m } };
            _mockContext.Setup(x => x.Claims
                .Include(c => c.User)
                .Where(c => c.Status == "Pending")
                .ToListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(claimsToReject);

            _mockClaimHub.Setup(x => x.Clients.All.SendAsync("UpdateClaimStatus", It.IsAny<int>(), "Rejected", It.IsAny<CancellationToken>())).Verifiable();

            var controller = new AdminDashboard(_mockContext.Object, _mockClaimHub.Object, new Mock<ILogger<AdminDashboard>>().Object);

            // Act
            await controller.AutoRejectClaimsForTesting(claimsToReject);

            // Assert
            _mockClaimHub.Verify(x => x.Clients.All.SendAsync("UpdateClaimStatus", It.IsAny<int>(), "Rejected", It.IsAny<CancellationToken>()), Times.Never);
            Assert.Equal("Pending", claimsToReject.First().Status);
        }
    
}