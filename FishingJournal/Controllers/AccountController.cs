using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FishingJournal.Models;
using FishingJournal.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FishingJournal.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger _logger;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public IAccountControllerWrappers Wrappers { get; set; }

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<AccountController> logger,
            IEmailSender emailSender,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _configuration = configuration;
            Wrappers = new AccountControllerWrappers();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            if (!Convert.ToBoolean(_configuration["EnableRegistration"]))
            {
                // If we got this far, something failed, redisplay form
                ViewData["Error"] = "Registration disabled!";
                return View();
            }
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new User {UserName = model.Email, Email = model.Email};
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Wrappers.GetActionLink(Url, nameof(ConfirmEmail), user, code);

                    var emailResult = _emailSender.SendEmailAsync(model.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                    TempData["Information"] = "Confirmation email sent.";
                    _logger.LogInformation("User created a new account with password.");
                    return RedirectToLocal(returnUrl);
                }

                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                TempData["Information"] = "Registration confirmed!";
                return RedirectToAction(nameof(Login));
            }

            TempData["Error"] = "Something went wrong.";
            return RedirectToAction(nameof(Register));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await Wrappers.SignOutAsync(HttpContext);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe,
                    lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return RedirectToLocal(returnUrl);
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await Wrappers.SignOutAsync(HttpContext);
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (returnUrl != null && Wrappers.IsLocalUrl(Url, returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Wrappers.GetActionLink(Url, nameof(ResetPassword), user, code);;

                var emailResult = _emailSender.SendEmailAsync(model.Email, "Reset your password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                TempData["Information"] = "Password reset email sent.";
                return RedirectToAction(nameof(Login));
            }

            TempData["Error"] = "No user with that email address found.";
            return RedirectToAction(nameof(Register));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var viewModel = new ResetPasswordViewModel {Email = user.Email, Code = code};

            return View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var code = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

                var emailResult = _emailSender.SendEmailAsync(model.Email, "Password reset notification",
                    $"Your password has been reset successfully.");
                TempData["Information"] = "Password reset successfully.";
                return RedirectToAction(nameof(Login));
            }

            TempData["Error"] = "No user with that email address found.";
            return RedirectToAction(nameof(Register));
        }

        public IActionResult ExternalLogin()
        {
            throw new NotImplementedException();
        }
    }

    public class AccountControllerWrappers : IAccountControllerWrappers
    {
        public async Task SignOutAsync(HttpContext context)
        {
            await context.SignOutAsync(IdentityConstants.ExternalScheme);
        }

        public string GetActionLink(IUrlHelper helper, string action, User user, string code)
        {
            return helper.ActionLink(action,
                values: new {userId = user.Id, code = code});
        }

        public bool IsLocalUrl(IUrlHelper helper, string url)
        {
            return helper.IsLocalUrl(url);
        }
    }

    public interface IAccountControllerWrappers
    {
        public Task SignOutAsync(HttpContext context);
        public string GetActionLink(IUrlHelper helper, string action, User user, string code);
        public bool IsLocalUrl(IUrlHelper helper, string url);
    }
}