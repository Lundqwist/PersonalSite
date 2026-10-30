// ════════════════════════════════════════════════════════════════════════════
// Areas/Admin/Controllers/CvController.cs — add, edit and delete CV entries.
//
//   GET  /Admin/Cv               list of all entries
//   GET  /Admin/Cv/Create        empty form          POST: save the new entry
//   GET  /Admin/Cv/Edit/5        form for entry 5    POST: save the changes
//   POST /Admin/Cv/Delete/5      delete entry 5
//   GET  /Admin/Cv/Profile       profile summary + skills   POST: save them
//
// This is the classic "CRUD" pattern: Create, Read, Update, Delete.
// ════════════════════════════════════════════════════════════════════════════

using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalSite.Data;
using PersonalSite.Models;

namespace PersonalSite.Areas.Admin.Controllers;

public class CvController : AdminControllerBase
{
    // The fields the edit form is allowed to change (an "allow list").
    // Anything else in the posted form — for example a faked "Id" field — is
    // ignored. This protects against "over-posting".
    // Each entry is a lambda pointing at a property, e.g. e => e.Title.
    private static readonly Expression<Func<CvEntry, object?>>[] EditableFields =
    {
        e => e.Type, e => e.Title, e => e.TitleEn,
        e => e.Organization, e => e.OrganizationEn, e => e.Employer,
        e => e.Location, e => e.LocationEn,
        e => e.StartDate, e => e.EndDate, e => e.DisplayDate, e => e.IsCurrent,
        e => e.Courses, e => e.CoursesEn,
        e => e.Comment, e => e.CommentEn,
        e => e.SortOrder,
    };

    private readonly AppDbContext _db;

    public CvController(AppDbContext db)
    {
        _db = db;
    }

    // The list, ordered by section and then by SortOrder.
    public async Task<IActionResult> Index()
    {
        var entries = await _db.CvEntries
            .OrderBy(e => e.Type)
            .ThenBy(e => e.SortOrder)
            .ToListAsync();

        return View(entries);
    }

    // Shows an empty form. "type" comes from the "+ Add" button's link
    // (?type=Course), so the right section is preselected.
    [HttpGet]
    public async Task<IActionResult> Create(CvEntryType type = CvEntryType.Workplace)
    {
        // New entries go last in their section by default.
        // "(int?)" allows MaxAsync to return null when the section is empty;
        // "?? 0" then uses 0 instead.
        var maxSort = await _db.CvEntries.Where(e => e.Type == type).MaxAsync(e => (int?)e.SortOrder) ?? 0;

        // Create and edit share the same view (Edit.cshtml).
        return View("Edit", new CvEntry { Type = type, SortOrder = maxSort + 1 });
    }

    // The new-entry form is posted here.
    [HttpPost]
    public async Task<IActionResult> Create()
    {
        var entry = new CvEntry();

        // TryUpdateModelAsync copies the allowed form fields into "entry" and
        // validates them. It returns false if something is invalid; we then
        // show the form again, with error messages and the typed values kept.
        if (!await TryUpdateModelAsync(entry, string.Empty, EditableFields))
            return View("Edit", entry);

        _db.CvEntries.Add(entry);          // mark as new
        await _db.SaveChangesAsync();      // INSERT into the database

        SetMessage($"Added \"{entry.Title}\".");   // \" = a quote character inside a string

        // "Post/Redirect/Get" pattern: after saving, redirect to the list, so
        // refreshing the page doesn't post the form a second time.
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var entry = await _db.CvEntries.FindAsync(id);
        return entry == null ? NotFound() : View(entry);
    }

    // C# can't have two methods with the same name and parameters, so the POST
    // version is named EditPost; [ActionName("Edit")] still maps it to /Edit.
    [HttpPost]
    [ActionName("Edit")]
    public async Task<IActionResult> EditPost(int id)
    {
        // Load the existing entry first, then copy only the allowed fields into it.
        var entry = await _db.CvEntries.FindAsync(id);
        if (entry == null)
            return NotFound();

        if (!await TryUpdateModelAsync(entry, string.Empty, EditableFields))
            return View("Edit", entry);

        // EF Core "tracks" entities it loaded: it knows which properties
        // changed and sends an UPDATE for just those.
        await _db.SaveChangesAsync();

        SetMessage($"Saved \"{entry.Title}\".");
        return RedirectToAction(nameof(Index));
    }

    // ── Profile summary and skills ──
    // Two PageContents rows (see Models/CvProfile.cs). The form has plain
    // fields, not a model, so the values are read by name; Body is English
    // and BodySv Swedish.

    // GET /Admin/Cv/Profile
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var summary = await LoadPageAsync(PageContent.CvSummaryKey);
        var skills = await LoadPageAsync(PageContent.CvSkillsKey);
        ViewData["SummaryEn"] = summary?.Body;
        ViewData["SummarySv"] = summary?.BodySv;
        ViewData["SkillsEn"] = skills?.Body;
        ViewData["SkillsSv"] = skills?.BodySv;
        return View();
    }

    [HttpPost]
    [ActionName("Profile")]
    public async Task<IActionResult> ProfilePost(string? summaryEn, string? summarySv, string? skillsEn, string? skillsSv)
    {
        await SavePageAsync(PageContent.CvSummaryKey, summaryEn, summarySv);
        await SavePageAsync(PageContent.CvSkillsKey, skillsEn, skillsSv);
        await _db.SaveChangesAsync();

        SetMessage("The profile and skills have been saved.");
        return RedirectToAction(nameof(Index));
    }

    private Task<PageContent?> LoadPageAsync(string key) =>
        _db.PageContents.FirstOrDefaultAsync(p => p.Key == key);

    // Updates the row, or adds it the first time
    private async Task SavePageAsync(string key, string? english, string? swedish)
    {
        var page = await LoadPageAsync(key);
        if (page == null)
        {
            page = new PageContent { Key = key };
            _db.PageContents.Add(page);
        }
        page.Body = english;
        page.BodySv = swedish;
        page.UpdatedAt = DateTime.UtcNow;
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var entry = await _db.CvEntries.FindAsync(id);
        if (entry != null)
        {
            _db.CvEntries.Remove(entry);   // mark for deletion
            await _db.SaveChangesAsync();  // DELETE from the database
            SetMessage($"Deleted \"{entry.Title}\".");
        }

        return RedirectToAction(nameof(Index));
    }
}
