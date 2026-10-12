// ════════════════════════════════════════════════════════════════════════════
// Models/MediaImage.cs — an image uploaded in the admin editor.
//
// The image itself is stored in the database (as bytes), together with a
// smaller thumbnail. Storing them in the database means they are backed up
// with everything else and no image folder has to be set up on the server.
// ════════════════════════════════════════════════════════════════════════════

using System.ComponentModel.DataAnnotations;

namespace PersonalSite.Models;

// Images are resized and re-encoded (WebP or JPEG) in the admin's browser
// before upload (see wwwroot/js/admin-editor.js), which also strips hidden
// metadata such as the GPS position from phone photos.
public class MediaImage
{
    // Size limits. MariaDB on the NAS accepts at most 1 MB per database
    // command by default (its "max_allowed_packet" setting), so each stored
    // image must stay below that. "700 * 1024" = 700 KB.
    public const int MaxImageBytes = 700 * 1024;
    public const int MaxThumbBytes = 200 * 1024;

    public int Id { get; set; }

    // The original file name, shown in the image library.
    [MaxLength(200)]
    public string FileName { get; set; } = string.Empty;

    // Description for screen readers (the "alt text").
    [MaxLength(300)]
    public string? AltText { get; set; }

    // "MIME type": tells the browser what kind of file it is, e.g. "image/webp".
    [MaxLength(50)]
    public string ContentType { get; set; } = "image/webp";

    public int Width { get; set; }

    public int Height { get; set; }

    // The image file itself. byte[] = an array of raw bytes; stored as a
    // "longblob" (binary large object) column.
    public byte[] Data { get; set; } = Array.Empty<byte>();

    [MaxLength(50)]
    public string ThumbContentType { get; set; } = "image/webp";

    // The smaller version used on cards and in the image library.
    public byte[] ThumbData { get; set; } = Array.Empty<byte>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // The addresses where Controllers/MediaController.cs serves the image.
    // "$"..."" is string interpolation: {Id} is replaced by the number.
    public string Url => $"/media/{Id}";
    public string ThumbUrl => $"/media/{Id}/thumb";
}
