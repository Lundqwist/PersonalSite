// ════════════════════════════════════════════════════════════════════════════
// Areas/Admin/Controllers/AboutController.cs — edit the About page.
//
//   GET  /Admin/About   show the editor
//   POST /Admin/About   save
//
// There is only one About page, so there is no list and no create/delete:
// the row is created automatically the first time you save.
// ════════════════════════════════════════════════════════════════════════════

using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalSite.Areas.Admin.Models;
using PersonalSite.Data;
using PersonalSite.Models;

namespace PersonalSite.Areas.Admin.Controllers;

public class AboutController : AdminControllerBase
{
    // Allowed form fields (see Admin/CvController.cs for why this list exists).
    private static readonly Expression<Func<PageContent, object?>>[] EditableFields =
    {
        p => p.Title, p => p.TitleSv,
        p => p.Body, p => p.BodySv,
    };

    private readonly AppDbContext _db;

    public AboutController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Load the About row, or start with an empty one if it doesn't exist yet.
        var page = await _db.PageContents.FirstOrDefaultAsync(p => p.Key == PageContent.AboutKey)
                   ?? new PageContent { Key = PageContent.AboutKey };
        return await EditView(page);
    }

    [HttpPost]
    [ActionName("Index")]
    public async Task<IActionResult> IndexPost()
    {
        var page = await _db.PageContents.FirstOrDefaultAsync(p => p.Key == PageContent.AboutKey);
        if (page == null)
        {
            // First save: create the row.
            page = new PageContent { Key = PageContent.AboutKey };
            _db.PageContents.Add(page);
        }

        if (!await TryUpdateModelAsync(page, string.Empty, EditableFields))
            return await EditView(page);

        page.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();   // INSERT or UPDATE, whichever is needed

        SetMessage("The About page has been saved.");
        return RedirectToAction(nameof(Index));
    }

    // The editor plus the image library (same as for posts).
    private async Task<IActionResult> EditView(PageContent page)
    {
        ViewData["Images"] = await MediaListItem.LoadAllAsync(_db);
        return View("Index", page);
    }
}
