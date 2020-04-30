using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TodoApp.Business.Smtp.Abstract;
using TodoApp.Business.Smtp.Models;
using TodoApp.Entities.Models;
using TodoApp.Models;
using TodoApp.Presentation.ViewModels;

namespace TodoApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IEmailSender emailSender;
        private readonly ILogger<AccountController> logger;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AccountController(
            IEmailSender emailSender,
            ILogger<AccountController> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            this.logger = logger;
            this.emailSender = emailSender;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl ?? string.Empty,
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList(),
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            model.ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);

                if (user?.EmailConfirmed == false
                    && (await userManager.CheckPasswordAsync(user, model.Password)))
                {
                    ModelState.AddModelError(string.Empty, "Email not confirmed yet!");
                    return View(model);
                }

                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl))
                        return LocalRedirect(model.ReturnUrl);
                    return RedirectToAction("index", "todos");
                }

                ModelState.AddModelError(key: string.Empty, "Pair username and password is wrong!");
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });

            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View("Login", model);
            }

            var info = await signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "Error loading external login information.");
                return View("Login", model);
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            ApplicationUser user = null;

            if (email != null)
            {
                user = await userManager.FindByEmailAsync(email);

                if (user?.EmailConfirmed == false)
                {
                    ModelState.AddModelError(string.Empty, "Email not confirmed yet!");
                    return View("Login", model);
                }
            }

            // this method checks existing record with received LoginProvider and ProviderKey in AspNetUserLogins
            var signInResult = await signInManager
                .ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            // if loging from external account is first, then create a record
            if (signInResult.Succeeded)
                return LocalRedirect(returnUrl);
            else
            {
                // check if external user has a local account
                if (email != null)
                {
                    if (user == null)
                    {
                        user = new ApplicationUser
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                        };

                        await userManager.CreateAsync(user);

                        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                        var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme);

                        await SendConfirmationLink(user);

                        ViewBag.ErrorTitle = "Registration successfull!";
                        ViewBag.ErrorMessage = "Before you can Login, please confirm your email, " +
                            "by clicking on the confirmation link we have emailed you";
                        return View("Error", new ErrorViewModel { RequestId = Request.HttpContext.TraceIdentifier });
                    }

                    await userManager.AddLoginAsync(user, info); // add login to AspNetUserLogins with received credentials
                    await signInManager.SignInAsync(user, isPersistent: false);

                    return LocalRedirect(returnUrl);
                }
                else
                    ModelState.AddModelError(string.Empty, $"Email claim not received from {info.LoginProvider}");
            }

            return View("Login", model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    Email = model.Email,
                    UserName = model.Email
                };

                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await SendConfirmationLink(user);

                    ViewBag.ErrorTitle = "Registration is successfull";
                    ViewBag.ErrorMessage = "Before you can Login, please confirm your email, " +
                        "by clicking on the confirmation link we have emailed you " + user.Email;
                    return View("Error", new ErrorViewModel {RequestId = Request.HttpContext.TraceIdentifier });
                }
                else
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("index", "home");
            }

            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var result = await userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return View();
            }

            ViewBag.ErrorTitle = "Email cannot be confirmed";
            return View("Error", new ErrorViewModel { RequestId = Request.HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            return user == null ? Json(true) : Json($"Email address is already taken");
        }

        private async Task SendConfirmationLink(ApplicationUser user)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);

            await emailSender.SendEmailAsync(
                new Message(to: new string[] { user.Email }, subject: "Email Confirmation",
                    content: $"<h1>Hi, {user.UserName}, please confirm your email by clicking on the link below: <a href='{confirmationLink}'>Confirm email</a></h1>"));
        }
    }
}