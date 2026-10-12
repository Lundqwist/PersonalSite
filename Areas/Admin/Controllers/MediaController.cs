// ════════════════════════════════════════════════════════════════════════════
// Areas/Admin/Controllers/MediaController.cs — image upload and delete.
//
//   POST /Admin/Media/Upload   receive an image + thumbnail, store them
//   POST /Admin/Media/Delete   delete an image
//
// These are called from JavaScript (admin-editor.js) with fetch(), not by
// normal form posts, so they answer with JSON (small data objects) instead
// of HTML pages.
//
// The browser has already resized and re-encoded the image; the server only
// checks that both files really are WebP or JPEG images within the size limits.
// ════════════════════════════════════════════════════════════════════════════

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalSite.Data;
using PersonalSite.Models;

namespace PersonalSite.Areas.Admin.Controllers;

public class MediaController : AdminControllerBase
{
    private readonly AppDbContext _db;

    public MediaController(AppDbContext db)
    {
        _db = db;
    }

    // [RequestSizeLimit]: reject requests larger than 2 MB before even reading them.
    // IFormFile = an uploaded file. The parameters are matched by name with
    // the fields the JavaScript sends in its FormData.
    [HttpPost]
    [RequestSizeLimit(2 * 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile? image, IFormFile? thumb, string? fileName, string? altText, int width, int height)
    {
        // BadRequest = HTTP 400 "your request is wrong". The anonymous object
        // becomes JSON: { "error": "..." }, which the JavaScript shows to the user.
        if (image == null || thumb == null)
            return BadRequest(new { error = "No image received." });
        if (image.Length > MediaImage.MaxImageBytes || thumb.Length > MediaImage.MaxThumbBytes)
            return BadRequest(new { error = "The image is too large." });

        var data = await ReadAsync(image);
        var thumbData = await ReadAsync(thumb);

        // Never trust the file name or declared type sent by the browser:
        // look at the actual bytes (see DetectImageType below).
        var contentType = DetectImageType(data);
        var thumbContentType = DetectImageType(thumbData);
        if (contentType == null || thumbContentType == null)
            return BadRequest(new { error = "Only WebP and JPEG images can be stored." });

        var media = new MediaImage
        {
            // Path.GetFileName strips any folder part, e.g. "C:\x\photo.jpg" -> "photo.jpg".
            FileName = Truncate(Path.GetFileName(fileName ?? "image"), 200),
            AltText = string.IsNullOrWhiteSpace(altText) ? null : Truncate(altText.Trim(), 300),
            ContentType = contentType,
            ThumbContentType = thumbContentType,
            ThumbData = thumbData,
            // Math.Clamp keeps the number between 0 and 10000.
            Width = Math.Clamp(width, 0, 10000),
            Height = Math.Clamp(height, 0, 10000),
        };

        // Saved in two steps (first the thumbnail, then the full image) so that
        // no single database command exceeds MariaDB's default 1 MB limit.
        // A "transaction" makes the two steps all-or-nothing: if the second
        // fails, the first is undone too. "await using" disposes it afterwards.
        await using (var tx = await _db.Database.BeginTransactionAsync())
        {
            _db.MediaImages.Add(media);
            await _db.SaveChangesAsync();   // INSERT row (with thumbnail); media.Id is now set
            media.Data = data;
            await _db.SaveChangesAsync();   // UPDATE the same row with the full image
            await tx.CommitAsync();         // make both steps permanent
        }

        // Tell the JavaScript where the new image can be found.
        return Json(new { id = media.Id, url = media.Url, thumbUrl = media.ThumbUrl, alt = media.AltText ?? "" });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        // ExecuteDeleteAsync runs "DELETE FROM MediaImages WHERE Id = ..."
        // directly, without loading the (large) image first. It returns how
        // many rows were deleted.
        // Posts using it as cover lose their cover (ON DELETE SET NULL, see AppDbContext).
        var deleted = await _db.MediaImages.Where(m => m.Id == id).ExecuteDeleteAsync();
        return deleted == 0 ? NotFound() : Json(new { id });
    }

    // Reads an uploaded file into a byte array in memory.
    // "using var" disposes (cleans up) the stream at the end of the method.
    private static async Task<byte[]> ReadAsync(IFormFile file)
    {
        using var ms = new MemoryStream((int)file.Length);
        await file.CopyToAsync(ms);
        return ms.ToArray();
    }

    // Identifies the image type from its first bytes, the "file signature"
    // (also called "magic number"), which every file format starts with:
    //   WebP: "RIFF" ....  "WEBP"
    //   JPEG: FF D8 FF  (hexadecimal numbers)
    // Returns null for anything else, so e.g. an HTML file renamed to .jpg is rejected.
    private static string? DetectImageType(byte[] d)
    {
        if (d.Length > 12 && d[0] == 'R' && d[1] == 'I' && d[2] == 'F' && d[3] == 'F'
            && d[8] == 'W' && d[9] == 'E' && d[10] == 'B' && d[11] == 'P')
            return "image/webp";
        if (d.Length > 3 && d[0] == 0xFF && d[1] == 0xD8 && d[2] == 0xFF)
            return "image/jpeg";
        return null;
    }

    // Shortens a text to at most "max" characters.
    // value[..max] is a "range": the characters from the start up to max.
    private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];
}
