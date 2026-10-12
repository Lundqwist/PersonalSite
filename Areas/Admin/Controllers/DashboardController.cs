// ════════════════════════════════════════════════════════════════════════════
// Areas/Admin/Controllers/DashboardController.cs — the admin overview (/Admin).
// Counts the content in each section for the tiles on the overview page.
// ════════════════════════════════════════════════════════════════════════════

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalSite.Data;
using PersonalSite.Models;

namespace PersonalSite.Areas.Admin.Controllers;

public class DashboardController : AdminControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        // CountAsync runs "SELECT COUNT(*) ..." — only the number is fetched.
        // The values are handed to the view through ViewData.
        ViewData["CvCount"] = await _db.CvEntries.CountAsync();
        ViewData["StoryCount"] = await _db.Posts.CountAsync(p => p.Category == PostCategory.Story);
        ViewData["ProjectCount"] = await _db.Posts.CountAsync(p => p.Category == PostCategory.Project);
        ViewData["PortfolioCount"] = await _db.Posts.CountAsync(p => p.Category == PostCategory.Portfolio);

        // When the About page was last saved ("?." gives null if there is no About page yet).
        ViewData["AboutUpdated"] = (await _db.PageContents.FirstOrDefaultAsync(p => p.Key == PageContent.AboutKey))?.UpdatedAt;
        return View();
    }
}
