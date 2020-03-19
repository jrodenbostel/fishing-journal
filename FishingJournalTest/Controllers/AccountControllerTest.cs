using System.Threading.Tasks;
using FishingJournal.Controllers;
using FishingJournal.Data;
using FishingJournal.Models;
using FishingJournal.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
            const string returnUrl = "/yay";
            
            var mock = new Mock<IAccountControllerWrappers>();
            mock.Setup(x => x.SignOutAsync(It.IsAny<HttpContext>())).Returns(Task.FromResult(""));

            var accountController = new AccountController(null, null, null, null, null) {Wrappers = mock.Object};

            //Act
            var response = accountController.Login(returnUrl).Result;

            //Assert
            //did the Wrappers SignOutAsync function get called?
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
            mockSignInManager.Verify(x => x.PasswordSignInAsync(It.Is<string>(s => s.Equals(email)),
                It.Is<string>(s => s.Equals(password)),
                It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
            Assert.Equal("Home", viewResult.ControllerName);
            Assert.Equal("Index", viewResult.ActionName);
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

        [Fact]
        public void ShouldRenderRegisterGet()
        {
            //Arrange
            const string returnUrl = "/yay";
            var accountController = new AccountController(null, null, null, null, null);

            //Act
            var response = accountController.Register(returnUrl);

            //Assert
            var viewResult = Assert.IsType<ViewResult>(response);
            Assert.Equal(returnUrl, viewResult.ViewData["ReturnUrl"]);
        }

        [Fact]
        public void ShouldBounceProcessRegisterPostForDisabledRegistration()
        {
            //Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(x => x["EnableRegistration"]).Returns("false");
            var accountController = new AccountController(null, null, null, null, mockConfiguration.Object);

            //Act
            var response = accountController.Register(null, null).Result;

            //Assert
            var viewResult = Assert.IsType<ViewResult>(response);
            Assert.Equal("Registration disabled!", viewResult.ViewData["Error"]);
        }

        [Fact]
        public void ShouldProcessRegisterPost()
        {
            //Arrange
            const string returnUrl = "/yay";
            var context = new DefaultHttpContext();
            var mockWrappers = new Mock<IAccountControllerWrappers>();
            var mockTempData = new TempDataDictionary(context, Mock.Of<ITempDataProvider>()) {["Information"] = ""};
            var viewModel = new RegisterViewModel
            {
                Email = "justin@rodenbostel.com",
                Password = "test_password1",
                ConfirmPassword = "test_password1"
            };
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(x => x["EnableRegistration"]).Returns("true");
            var mockLogger = new Mock<ILogger<AccountController>>();
            var mockContext = new Mock<IdentityContext>(new DbContextOptions<IdentityContext>());
            var mockUserStore = new Mock<UserStore<User>>(mockContext.Object, null);
            var mockUserManager =
                new Mock<UserManager<User>>(mockUserStore.Object, null, null, null, null, null, null, null, null);
            var mockEmailSender = new Mock<IEmailSender>();
            
            mockWrappers.Setup(x => x.GetActionLink(It.IsAny<IUrlHelper>(), It.IsAny<string>(), It.IsAny<User>(), It.IsAny<string>())).Returns("test");
            mockWrappers.Setup(x => x.IsLocalUrl(It.IsAny<IUrlHelper>(), It.IsAny<string>())).Returns(true);
            mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>())).ReturnsAsync("123456");
            mockEmailSender.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            var accountController = new AccountController(mockUserManager.Object, null, mockLogger.Object,
                mockEmailSender.Object, mockConfiguration.Object)
            {
                ControllerContext = {HttpContext = context}, Wrappers = mockWrappers.Object, TempData = mockTempData
                    
            };

            //Act
            var response = accountController.Register(viewModel, returnUrl).Result;

            //Assert
            mockUserManager.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
            mockUserManager.Verify(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()), Times.Once);
            mockEmailSender.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.IsType<RedirectResult>(response);
        }

        [Fact]
        public void ShouldProcessConfirmEmail()
        {
            //Arrange
            const string testCode = "test_code";
            var user = new User
            {
                Id = "1234",
                Email = "test@test.com"
            };
            var context = new DefaultHttpContext();
            var mockTempData = new TempDataDictionary(context, Mock.Of<ITempDataProvider>()) {["Information"] = ""};
            var mockContext = new Mock<IdentityContext>(new DbContextOptions<IdentityContext>());
            var mockUserStore = new Mock<UserStore<User>>(mockContext.Object, null);
            var mockUserManager =
                new Mock<UserManager<User>>(mockUserStore.Object, null, null, null, null, null, null, null, null);
            
            mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            mockUserManager.Setup(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            
            var accountController = new AccountController(mockUserManager.Object, null, null,
                null, null) {TempData = mockTempData};

            //Act
            var response = accountController.ConfirmEmail(user.Id, testCode).Result;
            
            //Assert
            var result = Assert.IsType<RedirectToActionResult>(response);
            mockUserManager.Verify(x => x.FindByIdAsync(user.Id), Times.Once);
            mockUserManager.Verify(x => x.ConfirmEmailAsync(user, testCode), Times.Once);
            Assert.Contains(nameof(accountController.Login), result.ActionName);
            Assert.Equal("Registration confirmed!", accountController.TempData["Information"]);
        }
        
        [Fact]
        public void ShouldProcessConfirmEmailFailure()
        {
            //Arrange
            const string testCode = "test_code";
            var user = new User
            {
                Id = "1234",
                Email = "test@test.com"
            };
            var context = new DefaultHttpContext();
            var mockTempData = new TempDataDictionary(context, Mock.Of<ITempDataProvider>()) {["Information"] = ""};
            var mockContext = new Mock<IdentityContext>(new DbContextOptions<IdentityContext>());
            var mockUserStore = new Mock<UserStore<User>>(mockContext.Object, null);
            var mockUserManager =
                new Mock<UserManager<User>>(mockUserStore.Object, null, null, null, null, null, null, null, null);
            
            mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            mockUserManager.Setup(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed());
            
            var accountController = new AccountController(mockUserManager.Object, null, null,
                null, null) {TempData = mockTempData};

            //Act
            var response = accountController.ConfirmEmail(user.Id, testCode).Result;
            
            //Assert
            var result = Assert.IsType<RedirectToActionResult>(response);
            mockUserManager.Verify(x => x.FindByIdAsync(user.Id), Times.Once);
            mockUserManager.Verify(x => x.ConfirmEmailAsync(user, testCode), Times.Once);
            Assert.Contains(nameof(accountController.Register), result.ActionName);
            Assert.Equal("Something went wrong.", accountController.TempData["Error"]);
        }

        [Fact]
        public void ShouldLogout()
        {
            var mockWrappers = new Mock<IAccountControllerWrappers>();
            var mockLogger = new Mock<ILogger<AccountController>>();
            
            mockWrappers.Setup(x => x.SignOutAsync(It.IsAny<HttpContext>())).Returns(Task.FromResult(""));

            var accountController = new AccountController(null, null, mockLogger.Object,
                null, null) {Wrappers = mockWrappers.Object};

            //Act
            var response = accountController.Logout().Result;
            
            //Assert
            var result = Assert.IsType<RedirectToActionResult>(response);
            mockWrappers.Verify(x => x.SignOutAsync(It.IsAny<HttpContext>()), Times.Once);
            Assert.Contains("Home", result.ControllerName);
            Assert.Contains(nameof(HomeController.Index), result.ActionName);
        }
        
        [Fact]
        public void ShouldRenderForgotPassword()
        {
            //Arrange
            var accountController = new AccountController(null, null, null, null, null);

            //Act
            var response = accountController.ForgotPassword();

            //Assert
            Assert.NotNull(response);
        }
    }
}