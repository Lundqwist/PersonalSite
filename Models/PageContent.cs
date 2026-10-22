// ════════════════════════════════════════════════════════════════════════════
// Models/PageContent.cs — editable text for a single page (the About page).
//
// Unlike posts, there is only one About page, so instead of a list we store
// one row identified by a Key ("About"). More pages could be added later with
// other keys.
// ════════════════════════════════════════════════════════════════════════════

using System.ComponentModel.DataAnnotations;

namespace PersonalSite.Models;

// Body is Markdown.
public class PageContent
{
    // The key of the About page row.
    public const string AboutKey = "About";

    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Key { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Title { get; set; }

    public string? Body { get; set; }

    // The old Swedish version of the page. No longer shown or edited (the site
    // is English only); kept so the saved text stays in the database.
    [MaxLength(200)]
    public string? TitleSv { get; set; }

    public string? BodySv { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
