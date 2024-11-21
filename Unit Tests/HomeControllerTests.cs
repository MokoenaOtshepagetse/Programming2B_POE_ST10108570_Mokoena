using Xunit;
using Microsoft.AspNetCore.Mvc;
using Claimed.Models;
using System;
using Claimed.Controllers;
using Moq;

namespace Claimed.UnitTests.Controllers
{
    public class HomeControllerTests
    {
        [Fact]
        public async Task Index_ReturnsRedirectToActionResult()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HomeController>>();
            var controller = new HomeController(loggerMock.Object);

            // Act
            var result = controller.Index() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Login", result.ControllerName);
            Assert.Equal("Index", result.ActionName);
        }

        [Fact]
        public async Task Privacy_ReturnsViewResult()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HomeController>>();
            var controller = new HomeController(loggerMock.Object);

            // Act
            var result = controller.Privacy() as ViewResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Error_ReturnsViewResult()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HomeController>>();
            var controller = new HomeController(loggerMock.Object);

            // Act
            var result = controller.Error() as ViewResult;

            // Assert
            Assert.NotNull(result);
        }
    }
}