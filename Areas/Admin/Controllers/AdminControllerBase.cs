// ════════════════════════════════════════════════════════════════════════════
// Areas/Admin/Controllers/AdminControllerBase.cs — shared base for all admin pages.
//
// An "Area" is a separate section of the site with its own controllers and
// views, here everything under /Admin. All admin controllers inherit from
// this class, so the two attributes below apply to every admin page.
// ════════════════════════════════════════════════════════════════════════════

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PersonalSite.Areas.Admin.Controllers;

// [Area("Admin")]   — these controllers belong to the Admin area (/Admin/...).
// [Authorize]       — only logged-in users may open them. Anyone else is sent
//                     to the login page (LoginPath in Program.cs).
//                     The login page itself opts out with [AllowAnonymous].
[Area("Admin")]
[Authorize]
public abstract class AdminControllerBase : Controller
{
    // Shows a green "saved" message on the next page.
    // TempData survives exactly one redirect, which is just what we need for
    // "save -> redirect to list -> show message". _AdminHeader.cshtml displays it.
    protected void SetMessage(string message) => TempData["AdminMessage"] = message;
}
