// ════════════════════════════════════════════════════════════════════════════
// Controllers/AboutController.cs — the public About page (/About).
// Loads the About text (edited on /Admin/About) and shows Views/About/Index.cshtml.
// ════════════════════════════════════════════════════════════════════════════

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalSite.Data;
using PersonalSite.Models;

namespace PersonalSite.Controllers;

public class AboutController : Controller
{
    private readonly AppDbContext _db;

    public AboutController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        // Find the row with Key "About". FirstOrDefaultAsync returns null if
        // there is none yet; the view then shows "Coming soon".
        // "p => p.Key == ..." is a lambda: a condition checked for each row.
        var page = await _db.PageContents.FirstOrDefaultAsync(p => p.Key == PageContent.AboutKey);
        return View(page);
    }
}
