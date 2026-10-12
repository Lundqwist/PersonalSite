// ════════════════════════════════════════════════════════════════════════════
// Areas/Admin/Controllers/AccountController.cs — log in, log out, change password.
//
//   GET  /Admin/Account/Login           show the login form
//   POST /Admin/Account/Login           check the username and password
//   POST /Admin/Account/Logout          log out
//   GET  /Admin/Account/ChangePassword  show the change-password form
//   POST /Admin/Account/ChangePassword  save a new password
//
// GET = the browser asks for a page. POST = the browser sends a form.
// ════════════════════════════════════════════════════════════════════════════

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PersonalSite.Areas.Admin.Models;
using PersonalSite.Data;
using PersonalSite.Models;

namespace PersonalSite.Areas.Admin.Controllers;

public class AccountController : AdminControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<AdminUser> _hasher;

    // Two services injected: the database and the password hasher.
    public AccountController(AppDbContext db, IPasswordHasher<AdminUser> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    // [AllowAnonymous] overrides [Authorize] from the base class: you must
    // be able to see the login page without being logged in.
    // returnUrl = the admin page you tried to open before being sent here.
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        // Already logged in? Go straight to the admin overview.
        // "?." stops with null if Identity is null instead of crashing.
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // The form is posted here. "model" is filled automatically from the form
    // fields (model binding) and validated using the attributes on LoginViewModel.
    // [EnableRateLimiting("login")]: max 5 attempts per minute (see Program.cs).
    [AllowAnonymous]
    [HttpPost]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // ModelState holds validation results, e.g. "Password is required".
        // If something is invalid, show the form again with the error messages.
        if (!ModelState.IsValid)
            return View(model);

        var user = await _db.AdminUsers.FirstOrDefaultAsync(u => u.UserName == model.UserName);

        // Check the password. When the username doesn't exist we still check
        // against a dummy hash, so both cases take the same time and give the
        // same message. Otherwise an attacker could learn which usernames exist.
        var result = _hasher.VerifyHashedPassword(
            user ?? new AdminUser(),
            user?.PasswordHash ?? DummyHash,
            model.Password);

        if (user == null || result == PasswordVerificationResult.Failed)
        {
            // string.Empty = a general error not tied to one field.
            ModelState.AddModelError(string.Empty, "Wrong username or password.");
            return View(model);
        }

        // If the hash was made with older settings, upgrade it now that we
        // know the correct password.
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _hasher.HashPassword(user, model.Password);
            await _db.SaveChangesAsync();
        }

        // "Claims" are facts about the logged-in user (id, name), stored
        // inside the encrypted login cookie.
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
            },
            CookieAuthenticationDefaults.AuthenticationScheme);

        // SignInAsync creates the login cookie and sends it to the browser.
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        // Back to the page the user wanted (only if it's on this site).
        return Url.IsLocalUrl(model.ReturnUrl)
            ? LocalRedirect(model.ReturnUrl!)
            : RedirectToAction("Index", "Dashboard");
    }

    // Logging out removes the cookie. It's a POST (a button in a form) so
    // another site can't log you out with a simple link.
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }

    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Get the logged-in user's id from the login cookie's claims.
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.AdminUsers.FindAsync(userId);   // FindAsync looks up by primary key
        if (user == null)
            return RedirectToAction(nameof(Login));           // nameof(Login) = "Login", checked by the compiler

        // The current password must be correct before it can be changed.
        if (_hasher.VerifyHashedPassword(user, user.PasswordHash, model.CurrentPassword) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), "The current password is wrong.");
            return View(model);
        }

        // Store a hash of the new password. EF Core noticed we changed
        // user.PasswordHash; SaveChangesAsync writes it with an UPDATE.
        user.PasswordHash = _hasher.HashPassword(user, model.NewPassword);
        await _db.SaveChangesAsync();

        SetMessage("Your password has been changed.");
        return RedirectToAction("Index", "Dashboard");
    }

    // Hash of a random value, used only to keep timing equal for unknown
    // usernames (see Login above). "static" = created once for the whole app.
    private static readonly string DummyHash =
        new PasswordHasher<AdminUser>().HashPassword(new AdminUser(), Guid.NewGuid().ToString());
}
