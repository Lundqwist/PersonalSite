// ════════════════════════════════════════════════════════════════════════════
// Models/PageContent.cs — editable text for a single page (the About page).
//
// Unlike posts, there is only one About page, so instead of a list we store
// one row identified by a Key ("About"). More pages could be added later with
// other keys.
// ════════════════════════════════════════════════════════════════════════════

using System.ComponentModel.DataAnnotations;

namespace PersonalSite.Models;

// English is the main language; the *Sv fields hold an optional Swedish
// version that falls back to English when empty. Body is Markdown.
public class PageContent
{
    // The key of the About page row.
    public const string AboutKey = "About";

    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Key { get; set; } = string.Empty;

    // English heading and text
    [MaxLength(200)]
    public string? Title { get; set; }

    // Swedish heading and text (optional)
    [MaxLength(200)]
    public string? TitleSv { get; set; }

    public string? Body { get; set; }

    public string? BodySv { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Returns the Swedish text for Swedish visitors when it exists,
    // otherwise the English text. "!en" means "not English".
    public string? GetTitle(bool en) => !en && !string.IsNullOrWhiteSpace(TitleSv) ? TitleSv : Title;
    public string? GetBody(bool en) => !en && !string.IsNullOrWhiteSpace(BodySv) ? BodySv : Body;
}
