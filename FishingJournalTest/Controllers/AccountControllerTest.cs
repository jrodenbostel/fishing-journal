using System.Threading.Tasks;
using FishingJournal.Controllers;
using FishingJournal.Data;
using FishingJournal.Models;
using FishingJournal.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace FishJournalTest.Controllers
{
    public class AccountControllerTest
    {
        [Fact]
        public void ShouldRenderLogin()
        {
            //Arrange
            var mock = new Mock<IAccountControllerWrappers>();
            const string returnUrl = "/yay";
            mock.Setup(x => x.SignOutAsync(It.IsAny<HttpContext>())).Returns(Task.FromResult(""));
            var accountController = new AccountController(null, null, null, null, null);
            accountController.Wrappers = mock.Object;

            //Act
            var response = accountController.Login(returnUrl).Result;

            //Assert
            //did the HttpContext SignOutAsync function get called?
            mock.Verify(x => x.SignOutAsync(It.IsAny<HttpContext>()), Times.Once);

            //do we have something for returnValue
            Assert.NotNull(response);

            //is the view data populated?
            var viewResult = Assert.IsType<ViewResult>(response);
            Assert.Equal(returnUrl, viewResult.ViewData["ReturnUrl"]);
        }

        [Fact]
        public void ShouldRespondToLoginPostForValidModel()
        {
            //Arrange
            const string email = "test@test.com";
            const string password = "password";
            var loginModel = new LoginViewModel
            {
                Email = email,
                Password = password,
                RememberMe = true
            };

            var mockContext = new Mock<IdentityContext>(new DbContextOptions<IdentityContext>());
            var mockUserStore = new Mock<UserStore<User>>(mockContext.Object, null);
            var mockUserManager =
                new Mock<UserManager<User>>(mockUserStore.Object, null, null, null, null, null, null, null, null);
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockUserClaimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            var mockSignInManager = new Mock<SignInManager<User>>(mockUserManager.Object,
                mockHttpContextAccessor.Object, mockUserClaimsPrincipalFactory.Object, null, null, null, null);
            var mockLogger = new Mock<ILogger<AccountController>>();


            mockSignInManager.Setup(x => x.PasswordSignInAsync(It.Is<string>(s => s.Equals(email)),
                It.Is<string>(s => s.Equals(password)),
                It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(SignInResult.Success);

            var accountController = new AccountController(mockUserManager.Object, mockSignInManager.Object,
                mockLogger.Object, null, null);

            //Act
            var response = accountController.Login(loginModel).Result;

            //Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(response);
            Assert.Equal("Home", viewResult.ControllerName);
            Assert.Equal("Index", viewResult.ActionName);
            mockSignInManager.Verify(x => x.PasswordSignInAsync(It.Is<string>(s => s.Equals(email)),
                It.Is<string>(s => s.Equals(password)),
                It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public void ShouldBounceToLoginGetForInvalidModel()
        {
            //Arrange
            const string email = "test@test.com";
            const string returnUrl = "/yay";

            var loginModel = new LoginViewModel {Email = email};
            var mock = new Mock<ILogger<AccountController>>().Object;
            var accountController = new AccountController(null, null, mock, null, null);
            accountController.ModelState.AddModelError("key", "error message");

            //Act
            var response = accountController.Login(loginModel, returnUrl).Result;

            //Assert
            var viewResult = Assert.IsType<ViewResult>(response);
            Assert.Equal(returnUrl, viewResult.ViewData["ReturnUrl"]);
            Assert.Equal(email, ((LoginViewModel) viewResult.Model).Email);
        }
    }
}