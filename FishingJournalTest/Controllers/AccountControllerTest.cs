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
using Microsoft.AspNetCore.Mvc.Routing;
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
        private Mock<IdentityContext> _mockContext;
        private Mock<UserStore<User>> _mockUserStore;
        private Mock<UserManager<User>> _mockUserManager;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private Mock<IUserClaimsPrincipalFactory<User>> _mockUserClaimsPrincipalFactory;
        private Mock<SignInManager<User>> _mockSignInManager;
        private Mock<ILogger<AccountController>> _mockLogger;

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
            Setup();

            const string email = "test@test.com";
            const string password = "password";
            var loginModel = new LoginViewModel
            {
                Email = email,
                Password = password,
                RememberMe = true
            };

            _mockSignInManager.Setup(x => x.PasswordSignInAsync(It.Is<string>(s => s.Equals(email)),
                It.Is<string>(s => s.Equals(password)),
                It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(SignInResult.Success);

            var accountController = new AccountController(_mockUserManager.Object, _mockSignInManager.Object,
                _mockLogger.Object, null, null);

            //Act
            var response = accountController.Login(loginModel).Result;

            //Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(response);
            _mockSignInManager.Verify(x => x.PasswordSignInAsync(It.Is<string>(s => s.Equals(email)),
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
            Setup();

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
            var mockEmailSender = new Mock<IEmailSender>();

            mockConfiguration.Setup(x => x["EnableRegistration"]).Returns("true");
            mockWrappers.Setup(x =>
                    x.GetActionLink(It.IsAny<IUrlHelper>(), It.IsAny<string>(), It.IsAny<User>(), It.IsAny<string>()))
                .Returns("test");
            mockWrappers.Setup(x => x.IsLocalUrl(It.IsAny<IUrlHelper>(), It.IsAny<string>())).Returns(true);
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>())).ReturnsAsync("123456");
            mockEmailSender.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            var accountController = new AccountController(_mockUserManager.Object, null, _mockLogger.Object,
                mockEmailSender.Object, mockConfiguration.Object)
            {
                ControllerContext = {HttpContext = context}, Wrappers = mockWrappers.Object, TempData = mockTempData
            };

            //Act
            var response = accountController.Register(viewModel, returnUrl).Result;

            //Assert
            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
            _mockUserManager.Verify(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()), Times.Once);
            mockEmailSender.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
            Assert.IsType<RedirectResult>(response);
        }

        [Fact]
        public void ShouldProcessConfirmEmail()
        {
            //Arrange
            Setup();

            const string testCode = "test_code";
            var user = new User
            {
                Id = "1234",
                Email = "test@test.com"
            };
            var context = new DefaultHttpContext();
            var mockTempData = new TempDataDictionary(context, Mock.Of<ITempDataProvider>()) {["Information"] = ""};

            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var accountController = new AccountController(_mockUserManager.Object, null, null,
                null, null) {TempData = mockTempData};

            //Act
            var response = accountController.ConfirmEmail(user.Id, testCode).Result;

            //Assert
            var result = Assert.IsType<RedirectToActionResult>(response);
            _mockUserManager.Verify(x => x.FindByIdAsync(user.Id), Times.Once);
            _mockUserManager.Verify(x => x.ConfirmEmailAsync(user, testCode), Times.Once);
            Assert.Contains(nameof(accountController.Login), result.ActionName);
            Assert.Equal("Registration confirmed!", accountController.TempData["Information"]);
        }

        [Fact]
        public void ShouldProcessConfirmEmailFailure()
        {
            //Arrange
            Setup();

            const string testCode = "test_code";
            var user = new User
            {
                Id = "1234",
                Email = "test@test.com"
            };
            var context = new DefaultHttpContext();
            var mockTempData = new TempDataDictionary(context, Mock.Of<ITempDataProvider>()) {["Information"] = ""};

            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed());

            var accountController = new AccountController(_mockUserManager.Object, null, null,
                null, null) {TempData = mockTempData};

            //Act
            var response = accountController.ConfirmEmail(user.Id, testCode).Result;

            //Assert
            var result = Assert.IsType<RedirectToActionResult>(response);
            _mockUserManager.Verify(x => x.FindByIdAsync(user.Id), Times.Once);
            _mockUserManager.Verify(x => x.ConfirmEmailAsync(user, testCode), Times.Once);
            Assert.Contains(nameof(accountController.Register), result.ActionName);
            Assert.Equal("Something went wrong.", accountController.TempData["Error"]);
        }

        [Fact]
        public void ShouldLogout()
        {
            Setup();

            _mockSignInManager.Setup(x => x.SignOutAsync()).Returns(Task.FromResult(""));

            var accountController = new AccountController(null, _mockSignInManager.Object, _mockLogger.Object,
                null, null);

            //Act
            var response = accountController.Logout().Result;

            //Assert
            var result = Assert.IsType<RedirectToActionResult>(response);
            _mockSignInManager.Verify(x => x.SignOutAsync(), Times.Once);
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

        [Fact]
        public void ForgotPasswordShouldFailWhenUserIsNotFound()
        {
            //Arrange
            Setup();

            var forgotPasswordViewModel = new ForgotPasswordViewModel
            {
                Email = "test@test.com"
            };
            var context = new DefaultHttpContext();
            var mockTempData = new TempDataDictionary(context, Mock.Of<ITempDataProvider>()) {["Error"] = ""};

            _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).Returns(Task.FromResult<User>(null));

            var accountController = new AccountController(_mockUserManager.Object, null, null,
                null, null) {TempData = mockTempData};

            //Act
            var response = accountController.ForgotPassword(forgotPasswordViewModel).Result;

            //Assert
            var result = Assert.IsType<RedirectToActionResult>(response);
            _mockUserManager.Verify(x => x.FindByEmailAsync(forgotPasswordViewModel.Email), Times.Once);
            Assert.Contains(nameof(accountController.Register), result.ActionName);
            Assert.Equal("No user with that email address found.", accountController.TempData["Error"]);
        }
        
        [Fact]
        public void ForgotPasswordShouldSucceed()
        {
            //Arrange
            Setup();

            const string email = "test@test.com";
            const string code = "1234";
            var forgotPasswordViewModel = new ForgotPasswordViewModel
            {
                Email = email
            };

            var user = new User
            {
                Email = email
            };
            var context = new DefaultHttpContext();
            var mockTempData = new TempDataDictionary(context, Mock.Of<ITempDataProvider>()) {["Information"] = ""};
            var mockWrappers = new Mock<IAccountControllerWrappers>();
            var mockEmailSender = new Mock<IEmailSender>();
            
            _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync(code);
            mockWrappers.Setup(x => x.GetActionLink(It.IsAny<IUrlHelper>(), It.IsAny<string>(), It.IsAny<User>(), It.IsAny<string>())).Returns("callbackUrl");
            mockEmailSender.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(""));

            var accountController = new AccountController(_mockUserManager.Object, null, null,
                mockEmailSender.Object, null) {Wrappers = mockWrappers.Object, TempData = mockTempData};

            //Act
            var response = accountController.ForgotPassword(forgotPasswordViewModel).Result;

            //Assert
            var result = Assert.IsType<RedirectToActionResult>(response);
            _mockUserManager.Verify(x => x.FindByEmailAsync(forgotPasswordViewModel.Email), Times.Once);
            _mockUserManager.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once);
            mockWrappers.Verify(x => x.GetActionLink(It.IsAny<IUrlHelper>(), It.IsAny<string>(), user, code), Times.Once);
            mockEmailSender.Verify(x => x.SendEmailAsync(email, "Reset your password", It.IsAny<string>()), Times.Once());
            Assert.Contains(nameof(accountController.Login), result.ActionName);
            Assert.Equal("Password reset email sent.", accountController.TempData["Information"]);
        }

        private void Setup()
        {
            _mockContext = new Mock<IdentityContext>(new DbContextOptions<IdentityContext>());
            _mockUserStore = new Mock<UserStore<User>>(_mockContext.Object, null);
            _mockUserManager =
                new Mock<UserManager<User>>(_mockUserStore.Object, null, null, null, null, null, null, null, null);
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockUserClaimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            _mockSignInManager = new Mock<SignInManager<User>>(_mockUserManager.Object,
                _mockHttpContextAccessor.Object, _mockUserClaimsPrincipalFactory.Object, null, null, null, null);
            _mockLogger = new Mock<ILogger<AccountController>>();
        }
    }
}