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

public class ClaimsControllerTests
{
    private ClaimsController _controller;
    private TestData _testData;
    private Mock<ClaimsDbContext> _mockContext;
    private Mock<IHubContext<ClaimHub>> _mockClaimHub;
    private Mock<IWebHostEnvironment> _mockHostingEnvironment;
    private Mock<ILogger<AdminDashboard>> _mockLogger;

    public ClaimsControllerTests()
    {
        _testData = new TestData();
        _mockContext = new Mock<ClaimsDbContext>();
        _mockClaimHub = new Mock<IHubContext<ClaimHub>>();
        _mockHostingEnvironment = new Mock<IWebHostEnvironment>();
        _mockLogger = new Mock<ILogger<AdminDashboard>>();

        _controller = new ClaimsController(
            _mockContext.Object,
            _mockClaimHub.Object,
            _mockHostingEnvironment.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithClaims()
    {
        // Arrange
        var claims = _testData.GetTestClaims();

        // Act
        var result = (ViewResult)await _controller.Index();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ViewResult>(result);
        Assert.Equal(claims.Count, ((List<Claim>)result.Model).Count);
    }

    [Fact]
    public async Task GenerateReport_ReturnsFileResult_WithPdf()
    {
        // Arrange
        var claims = _testData.GetTestClaims();

        // Act
        var result = await _controller.GenerateReport() as FileStreamResult;

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FileStreamResult>(result);
    }

    [Fact]
    public async Task GenerateReport_FilterByUserId_ReturnsFileResult_WithPdf()
    {
        // Arrange
        var claims = _testData.GetTestClaims();
        var userId = claims.First().UserId;

        // Act
        var result = await _controller.GenerateReport(userId: userId.ToString()) as FileStreamResult;

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FileStreamResult>(result);
        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal("report.pdf", result.FileDownloadName);
    }

    [Fact]
    public async Task GenerateReport_FilterByCourseId_ReturnsFileResult_WithPdf()
    {
        // Arrange
        var claims = _testData.GetTestClaims();
        var courseId = claims.First().Course;

        // Act
        var result = await _controller.GenerateReport(courseId: courseId);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FileStreamResult>(result);
        Assert.Equal("application/pdf", ((FileStreamResult)result).ContentType);
        Assert.Equal("report.pdf", ((FileStreamResult)result).FileDownloadName);
    }

    [Fact]
    public void GenerateReport_FilterByDateRange_ReturnsFileResult_WithPdf()
    {
        // Arrange
        var claims = _testData.GetTestClaims();
        var startDate = claims.First().DateSubmitted;
        var endDate = claims.Last().DateSubmitted;

        // Act
        var result = _controller.GenerateReport(startDate: startDate, endDate: endDate).Result as FileStreamResult;

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FileStreamResult>(result);
        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal("report.pdf", result.FileDownloadName);
    }

    [Fact]
    public async Task PrintInvoice_ReturnsFileResult_WithPdf()
    {
        // Arrange
        var invoice = new Invoice { InvoiceNumber = "INV001", InvoiceDate = DateTime.Now, ClaimId = 1, TotalAmount = 100 };

        // Act
        var result = await _controller.PrintInvoice(1) as FileStreamResult;

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FileStreamResult>(result);
        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal("invoice.pdf", result.FileDownloadName);
    }

    [Fact]
    public async Task CreateClaim_ValidClaim_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var claim = new Claim
        {
            Course = "Test Course",
            Description = "Test Description",
            HourlyRate = 100,
            HoursWorked = 2,
            SupportingDocument = "Test Document"
        };

        // Mock the necessary dependencies
        var mockContext = new Mock<ClaimsDbContext>();
        mockContext.Setup(x => x.Claims.Add(It.IsAny<Claim>())).Callback(() => claim.ClaimId = 1);
        mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var controller = new ClaimsController(mockContext.Object, new Mock<IHubContext<ClaimHub>>().Object, new Mock<IWebHostEnvironment>().Object, new Mock<ILogger<AdminDashboard>>().Object);

        // Act
        var result = await controller.CreateClaim(claim);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(1, claim.ClaimId);
        // Add more assertions as needed
    }

    [Fact]
    public async Task ApproveClaim_ValidClaimId_ReturnsRedirectToActionResult()
    {
        // Arrange
        var claimId = 1;
        var claim = new Claim { ClaimId = claimId, Status = "Pending" };

        // Mock the necessary dependencies
        var mockContext = new Mock<ClaimsDbContext>();
        mockContext.Setup(x => x.Claims.FindAsync(It.IsAny<int>())).ReturnsAsync(claim);
        mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var mockHub = new Mock<IHubContext<ClaimHub>>();
        mockHub.Setup(x => x.Clients.All.SendAsync("UpdateClaimStatus", claimId, "Approved", It.IsAny<CancellationToken>())).Verifiable();

        var controller = new ClaimsController(mockContext.Object, mockHub.Object, new Mock<IWebHostEnvironment>().Object, new Mock<ILogger<AdminDashboard>>().Object);

        // Act
        var result = await controller.ApproveClaim(claimId);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Dashboard", ((RedirectToActionResult)result).ActionName);
        mockHub.Verify(x => x.Clients.All.SendAsync("UpdateClaimStatus", claimId, "Approved", It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("Approved", claim.Status);
    }

    [Fact]
    public async Task RejectClaim_ValidClaimId_ReturnsRedirectToActionResult()
    {
        // Arrange
        var claimId = 1;
        var claim = new Claim { ClaimId = claimId, Status = "Pending" };

        // Mock the necessary dependencies
        var mockContext = new Mock<ClaimsDbContext>();
        mockContext.Setup(x => x.Claims.FindAsync(It.IsAny<int>())).ReturnsAsync(claim);
        mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var mockHub = new Mock<IHubContext<ClaimHub>>();
        mockHub.Setup(x => x.Clients.All.SendAsync("UpdateClaimStatus", claimId, "Rejected", It.IsAny<CancellationToken>())).Verifiable();

        var controller = new ClaimsController(mockContext.Object, mockHub.Object, new Mock<IWebHostEnvironment>().Object, new Mock<ILogger<AdminDashboard>>().Object);

        // Act
        var result = await controller.RejectClaim(claimId);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Dashboard", ((RedirectToActionResult)result).ActionName);
        mockHub.Verify(x => x.Clients.All.SendAsync("UpdateClaimStatus", claimId, "Rejected", It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("Rejected", claim.Status);
    }

    [Fact]
    public async Task RejectClaim_InvalidClaimId_ReturnsNotFoundResult()
    {
        // Arrange
        var claimId = 1;
        var mockContext = new Mock<ClaimsDbContext>();
        mockContext.Setup(x => x.Claims.FindAsync(It.IsAny<int>())).ReturnsAsync((Claim)null);

        var controller = new ClaimsController(mockContext.Object, new Mock<IHubContext<ClaimHub>>().Object, new Mock<IWebHostEnvironment>().Object, new Mock<ILogger<AdminDashboard>>().Object);

        // Act
        var result = await controller.RejectClaim(claimId);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithNoClaims()
    {
        // Arrange
        var claims = new List<Claim>();

        // Mock the necessary dependencies
        var mockContext = new Mock<ClaimsDbContext>();
        mockContext.Setup(x => x.Claims
            .Include(c => c.User)
            .OrderBy(c => c.SupportingDocument != null)
            .ThenByDescending(c => c.DateSubmitted)
            .ThenByDescending(c => c.DateReviewed.HasValue)
            .ThenByDescending(c => c.DateReviewed)
            .ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(claims);

        var controller = new ClaimsController(mockContext.Object, new Mock<IHubContext<ClaimHub>>().Object, new Mock<IWebHostEnvironment>().Object, new Mock<ILogger<AdminDashboard>>().Object);

        // Act
        var result = await controller.Index() as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ViewResult>(result);
        Assert.Empty((List<Claim>)result.Model);
    }

    [Fact]
    public async Task GenerateReport_NoClaimsMatchCriteria_ReturnsFileResult_WithPdf()
    {
    // Arrange
        var claims = new List<Claim>();

        // Mock the necessary dependencies
        var mockContext = new Mock<ClaimsDbContext>();
        mockContext.Setup(x => x.Claims
        .Include(c => c.User)
        .Where(c => c.UserId == -1)
        .ToListAsync(CancellationToken.None))
        .ReturnsAsync(claims);

        var controller = new ClaimsController(mockContext.Object, new Mock<IHubContext<ClaimHub>>().Object, new Mock<IWebHostEnvironment>().Object, new Mock<ILogger<AdminDashboard>>().Object);

        // Act
        var result = await controller.GenerateReport(userId: "non-existent-user-id") as FileStreamResult;

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FileStreamResult>(result);
        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal("report.pdf", result.FileDownloadName);
        }

    [Fact]
    public async Task CreateClaim_InvalidInput_ReturnsViewResult_WithModelError()
    {
    // Arrange
        var claim = new Claim
        {
        Course = "", // invalid input: empty string
        Description = "Test Description",
        HourlyRate = 100,
        HoursWorked = 2,
        SupportingDocument = "Test Document"
        };

        // Mock the necessary dependencies
        var mockContext = new Mock<ClaimsDbContext>();
        var controller = new ClaimsController(mockContext.Object, new Mock<IHubContext<ClaimHub>>().Object, new Mock<IWebHostEnvironment>().Object, new Mock<ILogger<AdminDashboard>>().Object);

        // Act
        var result = await controller.CreateClaim(claim, null);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ViewResult>(result);
        Assert.Single(controller.ModelState["Course"].Errors);
        Assert.Equal("The Course field is required.", controller.ModelState["Course"].Errors.First().ErrorMessage);
    }
}