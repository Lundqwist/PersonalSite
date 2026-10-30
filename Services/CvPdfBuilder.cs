// ════════════════════════════════════════════════════════════════════════════
// Services/CvPdfBuilder.cs — builds the downloadable CV as a PDF (/CV/Pdf).
//
// The PDF is made from the same database rows as the CV page, every time it is
// downloaded, so it is always up to date. It uses QuestPDF: the layout is
// described in C# as nested boxes (Column = stacked, Row = side by side),
// a bit like HTML and CSS.
//
// Fonts: the server (a Linux container) has no fonts installed, so the Lato
// font files in Fonts/ are copied next to the site and registered here.
// ════════════════════════════════════════════════════════════════════════════

using System.Text.RegularExpressions;
using PersonalSite.Models;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PersonalSite.Services;

public class CvPdfBuilder
{
    // The site's colors: navy for headings, orange for accents
    private const string Navy = "#0E2D57";
    private const string Orange = "#EC8F2E";
    private const string TextColor = "#222222";
    private const string Muted = "#666666";

    private const string Name = "Bjarne M. Lundqwist";

    // Registered once, when the service is created (it is a singleton, see Program.cs)
    public CvPdfBuilder()
    {
        // QuestPDF is free for individuals under its Community license
        QuestPDF.Settings.License = LicenseType.Community;
        // Only use our own fonts, so the PDF looks the same on every machine
        QuestPDF.Settings.UseSystemFonts = false;

        var fontFolder = Path.Combine(AppContext.BaseDirectory, "Fonts");
        foreach (var file in Directory.GetFiles(fontFolder, "*.ttf"))
        {
            using var stream = File.OpenRead(file);
            FontManager.RegisterFontFromStream(stream);
        }
    }

    // Returns the finished PDF file as bytes. "en" picks English or Swedish.
    public byte[] Build(IReadOnlyList<CvEntry> entries, CvProfile profile, bool en)
    {
        string T(string sv, string english) => en ? english : sv;

        // The sections in CV order: (type, heading)
        var sections = new[]
        {
            (CvEntryType.Workplace, T("Arbetslivserfarenhet", "Work experience")),
            (CvEntryType.School,    T("Utbildning", "Education")),
            (CvEntryType.Course,    T("Kurser", "Courses")),
            (CvEntryType.Board,     T("Styrelser & kommittéer", "Boards & committees")),
        };

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(2, Unit.Centimetre);
                page.MarginVertical(1.3f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontFamily("Lato").FontSize(9.5f).FontColor(TextColor).LineHeight(1.25f));

                page.Content().Column(column =>
                {
                    column.Spacing(11);

                    // ── Name and contact details ──
                    column.Item().Column(header =>
                    {
                        header.Item().Text(Name).FontSize(26).Bold().FontColor(Navy);
                        header.Item().Text("CURRICULUM VITAE").FontSize(9).Bold().FontColor(Orange).LetterSpacing(0.15f);
                        header.Item().PaddingTop(6).Text(text =>
                        {
                            text.DefaultTextStyle(x => x.FontSize(9).FontColor(Muted));
                            text.Span(T("Nässjö, Sverige", "Nässjö, Sweden"));
                            text.Span("   ·   ");
                            text.Hyperlink("bjarne@lundqwist.se", "mailto:bjarne@lundqwist.se");
                            text.Span("   ·   ");
                            text.Hyperlink("linkedin.com/in/lundqwist", "https://www.linkedin.com/in/lundqwist");
                            text.Span("   ·   ");
                            text.Hyperlink("lundqwist.se", "https://lundqwist.se");
                        });
                        header.Item().PaddingTop(10).LineHorizontal(2).LineColor(Orange);
                    });

                    // ── Profile summary ──
                    if (!string.IsNullOrWhiteSpace(profile.Summary))
                        column.Item().Text(profile.Summary).FontSize(10).LineHeight(1.35f);

                    // ── Skills: the group label on the left, the skills on the right ──
                    if (profile.Skills.Count > 0)
                    {
                        column.Item().ShowEntire().Column(skills =>
                        {
                            skills.Spacing(5);
                            skills.Item().Element(c => Heading(c, T("Kompetenser", "Skills")));
                            foreach (var group in profile.Skills)
                            {
                                skills.Item().Row(row =>
                                {
                                    row.ConstantItem(95).Text(group.Label ?? "").FontSize(8.5f).Bold().FontColor(Navy);
                                    row.RelativeItem().Text(string.Join("  ·  ", group.Items)).FontSize(9);
                                });
                            }
                        });
                    }

                    // ── One block per section that has entries ──
                    foreach (var (type, heading) in sections)
                    {
                        var items = entries.Where(e => e.Type == type).OrderBy(e => e.SortOrder).ToList();
                        if (items.Count == 0) continue;

                        // An employer (e.g. AcadeMedia) and its workplaces form one group;
                        // each group is split into blocks that may go on different pages
                        var blocks = CvEntry.GroupByEmployer(items).SelectMany(g => Blocks(g, en)).ToList();

                        column.Item().Column(section =>
                        {
                            section.Spacing(7);

                            // The heading is kept on the same page as the first block
                            // (ShowEntire), so it is never left alone at the bottom of a page
                            section.Item().ShowEntire().Column(first =>
                            {
                                first.Spacing(7);
                                first.Item().Element(c => Heading(c, heading));
                                first.Item().Element(blocks[0]);
                            });

                            foreach (var block in blocks.Skip(1))
                                section.Item().Element(block);
                        });
                    }
                });

                // ── Page footer: name and page number ──
                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text($"{Name} — CV").FontSize(8).FontColor(Muted);
                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(8).FontColor(Muted));
                        text.Span(T("Sida ", "Page "));
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            });
        })
        .WithMetadata(new DocumentMetadata
        {
            Title = $"{Name} — CV",
            Author = Name,
            Language = en ? "en-US" : "sv-SE",
        })
        .GeneratePdf();
    }

    // A section heading in capitals with a thin line under it.
    private static void Heading(IContainer container, string heading)
    {
        container.Column(h =>
        {
            h.Item().Text(heading.ToUpperInvariant()).FontSize(11).Bold().FontColor(Navy).LetterSpacing(0.08f);
            h.Item().PaddingTop(3).LineHorizontal(0.75f).LineColor("#D5DCE6");
        });
    }

    // The blocks a group is drawn as. A single entry is one block. An employer
    // is its name and full date range together with its first workplace, then
    // one block per further workplace; the workplaces sit behind an orange line.
    private static IEnumerable<Action<IContainer>> Blocks(CvEntryGroup group, bool en)
    {
        if (!group.IsEmployer)
        {
            yield return c => Entry(c, group.Entries[0], en);
            yield break;
        }

        yield return c => c.ShowEntire().Column(first =>
        {
            first.Spacing(5);
            first.Item().Row(row =>
            {
                row.ConstantItem(95).Text(group.GetDateRange(en)).FontSize(8.5f).FontColor(Muted);
                row.RelativeItem().Text(group.Employer).FontSize(11.5f).Bold().FontColor(Navy);
            });
            first.Item().Element(w => Workplace(w, group.Entries[0], en));
        });
        foreach (var entry in group.Entries.Skip(1))
            yield return c => Workplace(c, entry, en);
    }

    // A workplace inside an employer group, indented behind an orange line
    private static void Workplace(IContainer container, CvEntry entry, bool en) =>
        Entry(container.PaddingLeft(6).BorderLeft(1.5f).BorderColor(Orange).PaddingLeft(8), entry, en);

    // One CV entry: the dates on the left, the details on the right.
    private static void Entry(IContainer container, CvEntry entry, bool en)
    {
        var organization = entry.GetOrganization(en);
        var location = entry.GetLocation(en);
        // The course lines may start with "•" or "-" in the database; strip
        // that, since the PDF lists them on one line separated by " · "
        var courses = entry.GetCourses(en).Select(c => c.TrimStart('•', '-', '*', ' ')).ToArray();
        var comment = entry.GetComment(en);

        // ShowEntire: keep the whole entry on one page instead of splitting it
        container.ShowEntire().Row(row =>
        {
            row.ConstantItem(95).Text(entry.GetDateRange(en)).FontSize(8.5f).FontColor(Muted);

            row.RelativeItem().Column(details =>
            {
                details.Spacing(2);
                details.Item().Text(entry.GetTitle(en)).FontSize(10.5f).Bold().FontColor(Navy);

                // "LBS Kreativa Gymnasiet, Växjö"
                var place = string.Join(", ", new[] { organization, location }.Where(s => !string.IsNullOrWhiteSpace(s)));
                if (place.Length > 0)
                    details.Item().Text(place);

                if (courses.Length > 0)
                {
                    var label = entry.Type == CvEntryType.Workplace
                        ? (en ? "Courses taught: " : "Undervisade kurser: ")
                        : (en ? "Courses: " : "Kurser: ");
                    details.Item().PaddingTop(2).Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(8.5f));
                        text.Span(label).Bold();
                        text.Span(string.Join("  ·  ", courses));
                    });
                }

                // Blank lines between paragraphs are dropped in the PDF to save space
                if (!string.IsNullOrWhiteSpace(comment))
                    details.Item().PaddingTop(2).Text(Regex.Replace(comment.Trim(), @"(\r?\n\s*){2,}", "\n")).FontSize(8.5f).FontColor(Muted);
            });
        });
    }
}
