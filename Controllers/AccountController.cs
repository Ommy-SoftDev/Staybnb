using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Staybnb.Enums;
using Staybnb.Models;
using Staybnb.Services;
using Staybnb.ViewModels;

namespace Staybnb.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ActivityLogger _logger;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ActivityLogger logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                EmailConfirmed = true,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, vm.Password);
            if (result.Succeeded)
            {
                // Every new registrant starts as a Guest
                await _userManager.AddToRoleAsync(user, "Guest");
                await _logger.LogAsync(user.Id, "Registered a new account", ActivityType.Register);

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(vm);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginVM { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _signInManager.PasswordSignInAsync(
                vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(vm.Email);
                await _logger.LogAsync(user?.Id, "Logged in", ActivityType.Login);

                if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                    return Redirect(vm.ReturnUrl);

                if (user != null && (await _userManager.IsInRoleAsync(user, "Admin") || await _userManager.IsInRoleAsync(user, "SuperAdmin")))
                    return RedirectToAction("Dashboard", "Admin");

                if (user != null && await _userManager.IsInRoleAsync(user, "Host"))
                    return RedirectToAction("Dashboard", "Host");
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Email or password is incorrect.");
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _signInManager.SignOutAsync();
            await _logger.LogAsync(userId, "Logged out", ActivityType.Login);
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
