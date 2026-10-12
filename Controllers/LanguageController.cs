// ════════════════════════════════════════════════════════════════════════════
// Controllers/LanguageController.cs — the "Svenska | English" switch.
//
// The switch links to e.g. /Language/Set?culture=en-US&returnUrl=/CV
// This action saves the choice in a cookie and sends the visitor back to
// the page they were on. On the next requests, the localization step in
// Program.cs reads the cookie and switches the site language.
// ════════════════════════════════════════════════════════════════════════════

using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace PersonalSite.Controllers;

public class LanguageController : Controller
{
    // Only these values are accepted, so nobody can put arbitrary text in the cookie.
    private static readonly string[] SupportedCultures = { "sv-SE", "en-US" };

    // The parameters are filled from the query string (?culture=...&returnUrl=...)
    // by "model binding": ASP.NET Core matches them by name.
    public IActionResult Set(string culture, string? returnUrl)
    {
        if (SupportedCultures.Contains(culture))
        {
            // Write the standard ASP.NET Core language cookie, valid for one year.
            // IsEssential = it's needed for the site to work as the visitor asked.
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });
        }

        // Go back — but only to a page on THIS site. Without the IsLocalUrl
        // check, a link like ?returnUrl=https://evil.example could be used to
        // send visitors to another site (an "open redirect").
        // The "!" after returnUrl tells the compiler "I know this isn't null here".
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
    }
}
