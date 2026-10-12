// ════════════════════════════════════════════════════════════════════════════
// Models/CvEntry.cs — one line on the CV page (a job, course, school or board role).
//
// A "model" is a plain C# class describing data. EF Core maps it to a
// database table: the class CvEntry becomes the table "CvEntries", and each
// property becomes a column.
// ════════════════════════════════════════════════════════════════════════════

// Data annotations: the [Required], [MaxLength(...)] attributes below.
// They tell EF Core how to build the columns AND tell the admin forms what
// to validate (e.g. "Title must be filled in").
using System.ComponentModel.DataAnnotations;

// A namespace groups related classes, like a folder for code.
namespace PersonalSite.Models;

// An "enum" is a fixed list of named values. Using CvEntryType.Course instead
// of a free string like "course" means typos are caught by the compiler.
// In the database it is stored as text ("Workplace", "Course", ...), see AppDbContext.
public enum CvEntryType
{
    Workplace,
    Course,
    School,
    Board
}

// The CV is bilingual: the normal fields are Swedish, the *En fields hold the
// English version. If an English field is empty, the Swedish one is shown
// (useful for names like "LBS Kreativa Gymnasiet" that need no translation).
public class CvEntry
{
    // "Id" is automatically the primary key (unique number per row).
    // { get; set; } makes it a property you can read and change.
    public int Id { get; set; }

    [Required]
    public CvEntryType Type { get; set; }

    // "= string.Empty" gives a default value so it is never null.
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    // "string?" — the question mark means the value may be null (empty).
    // This is C#'s "nullable reference types" feature; the compiler warns you
    // if you use a nullable value without checking it first.
    [MaxLength(200)]
    public string? TitleEn { get; set; }

    [MaxLength(200)]
    public string? Organization { get; set; }

    [MaxLength(200)]
    public string? OrganizationEn { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(200)]
    public string? LocationEn { get; set; }

    // DateOnly = a date without a time of day. "?" = optional.
    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    // Free-text date shown instead of StartDate/EndDate when set,
    // e.g. "1992 – 1994" or a list of single days.
    [MaxLength(100)]
    public string? DisplayDate { get; set; }

    // True for things still going on; the CV then shows "Present" and a badge.
    public bool IsCurrent { get; set; } = false;

    // Courses taught, one per line — shown as a bullet list
    [MaxLength(1000)]
    public string? Courses { get; set; }

    [MaxLength(1000)]
    public string? CoursesEn { get; set; }

    // The notes shown when hovering over a CV card.
    [MaxLength(2000)]
    public string? Comment { get; set; }

    [MaxLength(2000)]
    public string? CommentEn { get; set; }

    // Order within its section: 1 is shown first.
    public int SortOrder { get; set; } = 0;

    // ── Helper methods used by the CV view ──
    // Each returns the English text when "en" is true and an English text
    // exists, otherwise the Swedish text.
    // "=>" is an "expression-bodied" method: a one-line method body.
    public string GetTitle(bool en) => en && !string.IsNullOrWhiteSpace(TitleEn) ? TitleEn : Title;
    public string? GetOrganization(bool en) => en && !string.IsNullOrWhiteSpace(OrganizationEn) ? OrganizationEn : Organization;
    public string? GetLocation(bool en) => en && !string.IsNullOrWhiteSpace(LocationEn) ? LocationEn : Location;
    public string? GetComment(bool en) => en && !string.IsNullOrWhiteSpace(CommentEn) ? CommentEn : Comment;

    // Splits the course text into lines, for the bullet list.
    // Returns an empty array when there are no courses, so the view can
    // simply check ".Length > 0".
    public string[] GetCourses(bool en)
    {
        var text = en && !string.IsNullOrWhiteSpace(CoursesEn) ? CoursesEn : Courses;
        return string.IsNullOrWhiteSpace(text)
            ? Array.Empty<string>()
            // Split on line breaks; drop empty lines and surrounding spaces.
            // "|" combines the two options (they are "flags").
            : text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
