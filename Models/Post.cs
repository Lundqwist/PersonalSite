// ════════════════════════════════════════════════════════════════════════════
// Models/Post.cs — a story, project or game page.
//
// All three kinds are stored in the same database table ("Posts") and told
// apart by the Category column. Each post is written in one language.
// ════════════════════════════════════════════════════════════════════════════

using System.ComponentModel.DataAnnotations;

namespace PersonalSite.Models;

// The kinds of posts. Stored as text ("Story", "Project", "Gaming") in the
// database (see AppDbContext), so adding a new kind needs no database change.
public enum PostCategory
{
    Story,
    Project,
    Gaming
}

// Names used for each category on the site and in the admin pages.
//
// These are "extension methods": the "this" before the first parameter means
// you can call them as if they belonged to the enum, e.g.
//     PostCategory.Story.Controller()   -> "Stories"
// Keeping the names in one place avoids repeating the same if/else in many views.
public static class PostCategoryInfo
{
    // Public controller and URL, e.g. /Stories
    // "switch" expression: picks a value depending on c. "_" means "anything else".
    public static string Controller(this PostCategory c) => c switch
    {
        PostCategory.Story => "Stories",
        PostCategory.Project => "Projects",
        _ => "Gaming",
    };

    // Admin labels (English)
    public static string AdminLabel(this PostCategory c) => c switch
    {
        PostCategory.Story => "Stories",
        PostCategory.Project => "Projects",
        _ => "Gaming",
    };

    // Used in phrases like "+ New story" and "Edit project".
    public static string AdminNoun(this PostCategory c) => c switch
    {
        PostCategory.Story => "story",
        PostCategory.Project => "project",
        _ => "game",
    };
}

// One post, written in one language (Swedish or English).
public class Post
{
    // "const" = a value that never changes. Used instead of typing "sv"/"en"
    // in many places.
    public const string Swedish = "sv";
    public const string English = "en";

    public int Id { get; set; }

    [Required]
    public PostCategory Category { get; set; }

    // ISO language code of the post: "sv" or "en".
    // [RegularExpression] rejects anything else when the admin form is posted.
    [Required, MaxLength(2), RegularExpression("^(sv|en)$", ErrorMessage = "Choose Swedish or English.")]
    public string Language { get; set; } = Swedish;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    // Short text shown on the card in the list.
    [MaxLength(500)]
    public string? Summary { get; set; }

    // The main text, written in Markdown (see Services/MarkdownRenderer.cs).
    public string? Body { get; set; }

    // Thumbnail in lists and header image on the post page.
    // CoverImageId is the "foreign key" (the number of a row in MediaImages);
    // CoverImage is a "navigation property" EF Core can fill with that row.
    // "int?" = may be null (no cover chosen).
    public int? CoverImageId { get; set; }

    public MediaImage? CoverImage { get; set; }

    // Optional link, e.g. a project's repository or a game's website.
    // [Url] only accepts http(s)/ftp addresses, which also blocks dangerous
    // "javascript:" links.
    [MaxLength(500), Url]
    public string? Url { get; set; }

    // Defaults for a new post: today's date, published.
    public DateOnly PublishedDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    // Unpublished posts are drafts: only visible in the admin.
    public bool IsPublished { get; set; } = true;

    // UTC = universal time, independent of time zone and summer time.
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Calculated properties (not stored in the database) ──
    // EF Core ignores properties that have only a getter ("=>").

    public bool IsEnglish => Language == English;

    // Label shown on the post: SE or EN
    public string LanguageLabel => IsEnglish ? "EN" : "SE";

    // The link's host name for display, e.g. "lundqwist.se".
    // Uri.TryCreate tries to parse the address; "out var uri" receives the
    // result. If parsing fails we return null.
    public string? UrlHost =>
        Uri.TryCreate(Url, UriKind.Absolute, out var uri) ? uri.Host.Replace("www.", "") : null;

    // The thumbnail to show in lists: the cover image, or else the first
    // image used in the text. Returns null when there is no image at all.
    public int? GetThumbnailImageId()
    {
        if (CoverImageId.HasValue)
            return CoverImageId;

        // Look for Markdown image syntax like ![description](/media/12)
        // and take the number. "??" means "if Body is null, use an empty string".
        var match = FirstImageRegex.Match(Body ?? string.Empty);
        return match.Success ? int.Parse(match.Groups[1].Value) : null;
    }

    // A regular expression (text pattern), created once and reused ("static readonly").
    //   !\[[^\]]*\]   matches  ![any description]
    //   \(/media/     matches  (/media/
    //   (\d+)         captures the digits (the image number) as "group 1"
    //   \)            matches  )
    // The @ before the string means backslashes are taken literally.
    private static readonly System.Text.RegularExpressions.Regex FirstImageRegex =
        new(@"!\[[^\]]*\]\(/media/(\d+)\)", System.Text.RegularExpressions.RegexOptions.Compiled);
}
