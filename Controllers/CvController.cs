// ════════════════════════════════════════════════════════════════════════════
// Controllers/CvController.cs — the public CV page (/CV) and its PDF (/CV/Pdf).
// Reads all CV entries from the database and passes them, grouped by
// section, to Views/CV/Index.cshtml — or turns them into a PDF.
// ════════════════════════════════════════════════════════════════════════════

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalSite.Data;
using PersonalSite.Models;
using PersonalSite.Services;

namespace PersonalSite.Controllers;

public class CvController : Controller
{
    // The database context. The underscore is a common naming style for
    // private fields.
    private readonly AppDbContext _db;
    private readonly CvPdfBuilder _pdf;

    // Dependency injection: ASP.NET Core sees that this constructor needs an
    // AppDbContext and a CvPdfBuilder and passes them in automatically
    // (registered in Program.cs).
    public CvController(AppDbContext db, CvPdfBuilder pdf)
    {
        _db = db;
        _pdf = pdf;
    }

    // "async Task<IActionResult>" + "await": the method can pause while it
    // waits for the database without blocking the server. Meanwhile the server
    // can handle other visitors' requests. The result arrives later.
    public async Task<IActionResult> Index()
    {
        var entries = await LoadEntriesAsync();

        // Group into a Dictionary: section type -> list of entries in it.
        // e.g. { Workplace: [...], Course: [...], School: [...], Board: [...] }
        // (This runs in memory, on the list we already loaded.)
        var grouped = entries
            .GroupBy(e => e.Type)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Pass the dictionary to the view as its "Model".
        return View(grouped);
    }

    // GET /CV/Pdf?lang=sv — the CV as a PDF download (English unless lang=sv).
    public async Task<IActionResult> Pdf(string? lang)
    {
        var en = lang != "sv";
        var bytes = _pdf.Build(await LoadEntriesAsync(), en);

        // File(...) sends the bytes; the file name makes the browser download it
        return File(bytes, "application/pdf", en ? "Bjarne_Lundqwist_CV_EN.pdf" : "Bjarne_Lundqwist_CV_SV.pdf");
    }

    // LINQ query, translated by EF Core into SQL:
    //   SELECT * FROM CvEntries ORDER BY Type, SortOrder
    // Nothing is sent to the database until ToListAsync() runs it.
    private Task<List<CvEntry>> LoadEntriesAsync() =>
        _db.CvEntries
            .OrderBy(e => e.Type)
            .ThenBy(e => e.SortOrder)
            .ToListAsync();
}
