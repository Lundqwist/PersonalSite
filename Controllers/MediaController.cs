// ════════════════════════════════════════════════════════════════════════════
// Controllers/MediaController.cs — serves uploaded images to visitors.
//
//   /media/12        -> the full image with id 12
//   /media/12/thumb  -> its thumbnail
//
// Uploading and deleting images is done in Areas/Admin/Controllers/MediaController.cs.
// ════════════════════════════════════════════════════════════════════════════

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalSite.Data;

namespace PersonalSite.Controllers;

public class MediaController : Controller
{
    private readonly AppDbContext _db;

    public MediaController(AppDbContext db)
    {
        _db = db;
    }

    // "Attribute routing": [HttpGet("/media/{id:int}")] sets the URL directly
    // on the action, instead of the default /{controller}/{action} pattern.
    // "{id:int}" only matches numbers.
    [HttpGet("/media/{id:int}")]
    public async Task<IActionResult> Image(int id)
    {
        // AsNoTracking: we only read, so EF Core doesn't need to watch for changes.
        // Select(...) fetches ONLY the two columns we need (not the thumbnail),
        // creating an "anonymous type" object: new { m.Data, m.ContentType }.
        var image = await _db.MediaImages.AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new { m.Data, m.ContentType })
            .FirstOrDefaultAsync();

        // NotFound() = HTTP status 404.
        return image == null ? NotFound() : Cached(image.Data, image.ContentType);
    }

    [HttpGet("/media/{id:int}/thumb")]
    public async Task<IActionResult> Thumb(int id)
    {
        var image = await _db.MediaImages.AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new { m.ThumbData, m.ThumbContentType })
            .FirstOrDefaultAsync();

        return image == null ? NotFound() : Cached(image.ThumbData, image.ThumbContentType);
    }

    // Sends the image bytes with response headers:
    //   Cache-Control: the browser may keep the image for a year (31536000 s)
    //                  without asking again. Safe because an uploaded image
    //                  never changes; a new upload gets a new id.
    //   X-Content-Type-Options: nosniff — the browser must treat the file as
    //                  the declared image type and never guess (security).
    private FileContentResult Cached(byte[] data, string contentType)
    {
        Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        Response.Headers.XContentTypeOptions = "nosniff";
        return File(data, contentType);
    }
}
