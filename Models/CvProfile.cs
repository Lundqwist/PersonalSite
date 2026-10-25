// ════════════════════════════════════════════════════════════════════════════
// Models/CvProfile.cs — the profile summary and skills at the top of the CV.
//
// Not a table of its own: the text is stored as two PageContents rows, keys
// "CvSummary" and "CvSkills" (Body = English, BodySv = Swedish), and edited on
// /Admin/Cv/Profile. This class picks the chosen language and splits the
// skills text into groups, for the CV page and the PDF.
// ════════════════════════════════════════════════════════════════════════════

namespace PersonalSite.Models;

// One skills line, e.g. "Programming: C#, .NET, C++" -> label "Programming",
// items ["C#", ".NET", "C++"]. "record" = a small class that just holds values.
public record SkillGroup(string? Label, string[] Items);

public class CvProfile
{
    public string? Summary { get; init; }
    public List<SkillGroup> Skills { get; init; } = new();

    public bool IsEmpty => string.IsNullOrWhiteSpace(Summary) && Skills.Count == 0;

    // Builds the profile from the two rows (either may be null). English falls
    // back to Swedish when there is no English text, like the CV entries.
    public static CvProfile From(PageContent? summary, PageContent? skills, bool en) => new()
    {
        Summary = Pick(summary, en)?.Trim(),
        Skills = ParseSkills(Pick(skills, en)),
    };

    private static string? Pick(PageContent? page, bool en) =>
        en && !string.IsNullOrWhiteSpace(page?.Body) ? page.Body : page?.BodySv;

    // One group per line: "Label: item, item, item". A line without a colon
    // becomes a group without a label.
    private static List<SkillGroup> ParseSkills(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new();

        var groups = new List<SkillGroup>();
        foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var colon = line.IndexOf(':');
            var label = colon > 0 ? line[..colon].Trim() : null;
            var rest = colon > 0 ? line[(colon + 1)..] : line;
            var items = rest.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (items.Length > 0)
                groups.Add(new SkillGroup(label, items));
        }
        return groups;
    }
}
