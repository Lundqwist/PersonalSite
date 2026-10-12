// ════════════════════════════════════════════════════════════════════════════
// Controllers/CvController.cs — the public CV page (/CV).
// Reads all CV entries from the database and passes them, grouped by
// section, to Views/CV/Index.cshtml.
// ════════════════════════════════════════════════════════════════════════════

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalSite.Data;

namespace PersonalSite.Controllers;

public class CvController : Controller
{
    // The database context. The underscore is a common naming style for
    // private fields.
    private readonly AppDbContext _db;

    // Dependency injection: ASP.NET Core sees that this constructor needs an
    // AppDbContext and passes one in automatically (registered in Program.cs).
    public CvController(AppDbContext db)
    {
        _db = db;
    }

    // "async Task<IActionResult>" + "await": the method can pause while it
    // waits for the database without blocking the server. Meanwhile the server
    // can handle other visitors' requests. The result arrives later.
    public async Task<IActionResult> Index()
    {
        // LINQ query, translated by EF Core into SQL:
        //   SELECT * FROM CvEntries ORDER BY Type, SortOrder
        // Nothing is sent to the database until ToListAsync() runs it.
        var entries = await _db.CvEntries
            .OrderBy(e => e.Type)
            .ThenBy(e => e.SortOrder)
            .ToListAsync();

        // Group into a Dictionary: section type -> list of entries in it.
        // e.g. { Workplace: [...], Course: [...], School: [...], Board: [...] }
        // (This runs in memory, on the list we already loaded.)
        var grouped = entries
            .GroupBy(e => e.Type)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Pass the dictionary to the view as its "Model".
        return View(grouped);
    }
}
