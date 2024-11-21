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

public class AdminDashboardControllerTests
{
    private AdminDashboard _controller;
    private TestData _testData;
    private Mock<ClaimsDbContext> _mockContext;
    private Mock<IHubContext<ClaimHub>> _mockClaimHub;
    private Mock<IWebHostEnvironment> _mockHostingEnvironment;
    private Mock<ILogger<AdminDashboard>> _mockLogger;

    public AdminDashboardControllerTests()
    {
        _testData = new TestData();
        _mockContext = new Mock<ClaimsDbContext>();
        _mockClaimHub = new Mock<IHubContext<ClaimHub>>();
        _mockHostingEnvironment = new Mock<IWebHostEnvironment>();
        _mockLogger = new Mock<ILogger<AdminDashboard>>();

        _controller = new AdminDashboard(
            _mockContext.Object,
            _mockClaimHub.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task AdminDashboard_ReturnsViewResult_WithPendingClaims()
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
        Assert.Equal("AdminDashboard", result.ViewName);
        Assert.Equal(pendingClaims, result.Model);
    }

    [Fact]
    public async Task AdminDashboard_ReturnsViewResult_WithNoPendingClaims()
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
        Assert.Equal("AdminDashboard", result.ViewName);
        Assert.Empty((List<Claim>)result.Model);
    }
}