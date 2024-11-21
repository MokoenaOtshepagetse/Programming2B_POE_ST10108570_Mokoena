using Xunit;
using Moq;
using Claimed.Models;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Claimed.UnitTests.Services
{
    public class ServiceTests
    {
        [Fact]
        public async Task InvoiceService_GenerateInvoice_ReturnsInvoice()
        {
            // Arrange
            var claim = new Claim { ClaimId = 1, UserId = 1, Course = "Course1", HourlyRate = 1005, HoursWorked = 2 };

            var mockContext = new Mock<ClaimsDbContext>();
            mockContext.Setup(x => x.Claims.ToListAsync(It.IsAny<CancellationToken>()))
          .ReturnsAsync(new List<Claim> { claim });

            var mockClaimHub = new Mock<IHubContext<ClaimHub>>();

            var service = new InvoiceService(mockContext.Object);

            // Act
            var result = await service.GenerateInvoice(claim);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20, result.Total);
        }

        [Fact]
        public void ClaimCalculator_CalculatePayment_ReturnsPayment()
        {
            // Arrange
            var claim = new Claim { ClaimId = 1, UserId = 1, Course = "Course1", HourlyRate = 1005, HoursWorked = 2 };

            var calculator = new ClaimCalculator();

            // Act
            var result = calculator.CalculatePayment(claim);

            // Assert
            Assert.Equal(2010, result);
        }

        [Fact]
        public void ClaimCalculator_calculateClaimAmount_ReturnsClaimAmount()
        {
            // Arrange
            var calculator = new ClaimCalculator();

            // Act
            var result = calculator.calculateClaimAmount(2, 1005);

            // Assert
            Assert.Equal(1809, result);
        }

        [Fact]
        public async Task ClaimHub_UpdateClaimStatus_UpdatesClaimStatus()
        {
            // Arrange
            var claimId = 1;
            var status = "Approved";
            var mockLogger = new Mock<ILogger<ClaimHub>>();
            var mockHub = new Mock<IHubContext<ClaimHub>>();
            mockHub.Setup(x => x.Clients.All.SendAsync("UpdateClaimStatus", claimId, status, It.IsAny<CancellationToken>())).Verifiable();

            var hub = new ClaimHub(mockLogger.Object);

            // Act
            await hub.UpdateClaimStatus(claimId, status);

            // Assert
            mockHub.Verify(x => x.Clients.All.SendAsync("UpdateClaimStatus", claimId, status, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}