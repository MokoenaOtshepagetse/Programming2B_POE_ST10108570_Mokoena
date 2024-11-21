using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Moq;
using Claimed.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Claimed.UnitTests.Controllers
{
    public class LoginControllerTests
    {
        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            // Arrange
            var controller = new LoginController(new Mock<ClaimsDbContext>().Object);

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsRedirectToActionResult()
        {
            // Arrange
            var mockContext = new Mock<ClaimsDbContext>();
            var user = new User { Username = "test", PasswordHash = "password" };
            mockContext.Setup(x => x.Users.FirstOrDefault(u => u.Username == "test" && u.PasswordHash == "password")).Returns(user);
            var controller = new LoginController(mockContext.Object);

            // Act
            var result = await controller.Login(new LoginViewModel { Username = "test", Password = "password" }) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("AdminDashboard", result.ControllerName);
            Assert.Equal("Index", result.ActionName);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsViewResultWithError()
        {
            // Arrange
            var mockContext = new Mock<ClaimsDbContext>();
            var controller = new LoginController(mockContext.Object);

            // Act
            var result = await controller.Login(new LoginViewModel { Username = "test", Password = "wrongpassword" }) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ViewName);
            Assert.Contains("Invalid credentials.", controller.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
        }

        [Fact]
        public async Task Login_ModelStateInvalid_ReturnsViewResultWithError()
        {
            // Arrange
            var mockContext = new Mock<ClaimsDbContext>();
            var controller = new LoginController(mockContext.Object);
            var model = new LoginViewModel { Username = "", Password = "" };
            controller.ModelState.AddModelError("error", "error");

            // Act
            var result = await controller.Login(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ViewName);
            Assert.Contains("error", controller.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
        }
    }
}