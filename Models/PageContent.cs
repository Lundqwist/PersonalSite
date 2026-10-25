// ════════════════════════════════════════════════════════════════════════════
// Models/PageContent.cs — editable text for a single page (the About page, and
// the profile summary and skills on the CV).
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

    // The CV's profile summary and skills (see CvProfile.cs). For these rows
    // Body is English and BodySv is Swedish; Title is not used.
    public const string CvSummaryKey = "CvSummary";
    public const string CvSkillsKey = "CvSkills";

    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Key { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Title { get; set; }

    public string? Body { get; set; }

    // The Swedish version. For the About page it is no longer shown or edited
    // (the site is English only) and is kept so the saved text stays in the
    // database; the CV rows above use BodySv for their Swedish text.
    [MaxLength(200)]
    public string? TitleSv { get; set; }

    public string? BodySv { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
