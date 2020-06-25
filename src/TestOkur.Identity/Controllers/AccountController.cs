namespace TestOkur.Identity.Controllers
{
    using IdentityServer4;
    using IdentityServer4.Events;
    using IdentityServer4.Extensions;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using TestOkur.Identity.Infrastructure;
    using TestOkur.Identity.Infrastructure.Data;
    using TestOkur.Identity.Models;

    [SecurityHeaders]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEventService _events;
        private readonly ApplicationDbContext _dbContext;
        private readonly IIdentityServerInteractionService _interaction;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEventService events,
            ApplicationDbContext dbContext,
            IIdentityServerInteractionService interaction)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _events = events;
            _dbContext = dbContext;
            _interaction = interaction;
        }

        [HttpGet("[Controller]/reset-password")]
        public IActionResult ResetPassword([RequiredFromQuery] string token, [RequiredFromQuery] string email)
        {
            return View(new ResetPasswordModel());
        }

        [HttpPost("[Controller]/reset-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model, [RequiredFromQuery] string token, [RequiredFromQuery] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var result = await _userManager.ResetPasswordAsync(
                user,
                token,
                model.NewPassword);

            if (result.Succeeded)
            {
                _dbContext.ActivityLogs.Add(new ActivityLog(ActivityLogType.PasswordReset, user.Id, User.Identity.Name));
                await _dbContext.SaveChangesAsync();
                return View(new ResetPasswordModel(true));
            }

            return BadRequest(result.Errors);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel(returnUrl));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Username,
                model.Password,
                model.RememberLogin,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var loggedInUser = await _userManager.FindByNameAsync(model.Username);
                var isAdmin = await _userManager.IsInRoleAsync(loggedInUser, Roles.Admin);

                if (isAdmin)
                {
                    var props = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberLogin,
                        ExpiresUtc = DateTimeOffset.UtcNow.Add(TimeSpan.FromDays(30)),
                    };
                    await HttpContext.SignInAsync(
                        new IdentityServerUser(loggedInUser.Id)
                    {
                        DisplayName = loggedInUser.UserName,
                    }, props);

                    await _events.RaiseAsync(
                        new UserLoginSuccessEvent(
                            loggedInUser.UserName,
                            loggedInUser.Id,
                            loggedInUser.UserName));
                    return Redirect(model.ReturnUrl);
                }
            }

            await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials"));
            return BadRequest(ErrorCodes.InvalidUsernameOrPassword);
        }

        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                await _signInManager.SignOutAsync();
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            return View(new LoggedOutViewModel(logout?.PostLogoutRedirectUri));
        }
    }
}
