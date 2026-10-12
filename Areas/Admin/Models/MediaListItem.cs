// ════════════════════════════════════════════════════════════════════════════
// Areas/Admin/Models/MediaListItem.cs — one image in the editor's image library.
// ════════════════════════════════════════════════════════════════════════════

using Microsoft.EntityFrameworkCore;
using PersonalSite.Data;

namespace PersonalSite.Areas.Admin.Models;

// A "record" is a compact class mainly for holding data. This one-line form
// ("positional record") creates the properties Id, FileName and AltText and
// a constructor that sets them: new MediaListItem(5, "photo.jpg", "A photo").
//
// We use it instead of MediaImage so the library only loads names and ids,
// not the (large) image bytes.
public record MediaListItem(int Id, string FileName, string? AltText)
{
    public string Url => $"/media/{Id}";
    public string ThumbUrl => $"/media/{Id}/thumb";

    // All images, newest first.
    // .Select(...) makes EF Core fetch only these three columns from the database.
    // "static" = called on the type itself: MediaListItem.LoadAllAsync(_db).
    public static Task<List<MediaListItem>> LoadAllAsync(AppDbContext db) =>
        db.MediaImages.AsNoTracking()
            .OrderByDescending(m => m.Id)
            .Select(m => new MediaListItem(m.Id, m.FileName, m.AltText))
            .ToListAsync();
}
