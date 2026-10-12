// ════════════════════════════════════════════════════════════════════════════
// Services/MarkdownRenderer.cs — turns post text written in Markdown into HTML.
//
// Markdown is a simple way to format text with ordinary characters:
//     **bold**   *italic*   ## Heading   - list item   [link](https://...)
// The browser can't show Markdown, so we convert it to HTML on the server.
//
// SECURITY: HTML from user-written text could contain harmful code, such as a
// <script> that steals the admin login cookie (an attack called XSS). So we:
//   1. tell the Markdown converter to ignore any raw HTML in the text, and
//   2. run the result through an HTML "sanitizer" that removes everything
//      except safe formatting, normal links and images.
//
// Libraries used: Markdig (Markdown -> HTML) and HtmlSanitizer (cleaning).
// ════════════════════════════════════════════════════════════════════════════

using Ganss.Xss;                 // HtmlSanitizer
using Markdig;                   // Markdown converter
using Microsoft.AspNetCore.Html; // HtmlString

namespace PersonalSite.Services;

// Registered as a Singleton in Program.cs: one instance is shared by all
// requests. That's safe because it only holds settings, no per-visitor data.
public class MarkdownRenderer
{
    // The Markdown "pipeline" = the converter plus the extras we switch on.
    // "readonly" fields can only be set once, when the object is created.
    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder()
        .UseEmphasisExtras()                 // ~~strikethrough~~ and similar
        .UsePipeTables()                     // simple tables with | columns |
        .UseListExtras()                     // more list styles (a. b. c.)
        .UseAutoLinks()                      // turns https://... into a link automatically
        .UseSoftlineBreakAsHardlineBreak()   // a single line break stays a line break
        .DisableHtml()                       // ignore raw HTML typed into the text
        .Build();

    private readonly HtmlSanitizer _sanitizer;

    // The constructor runs once, when the object is created.
    public MarkdownRenderer()
    {
        _sanitizer = new HtmlSanitizer();

        // Only allow these kinds of link addresses. This blocks tricks like
        // href="javascript:..." which would run code when clicked.
        _sanitizer.AllowedSchemes.Clear();
        _sanitizer.AllowedSchemes.Add("http");
        _sanitizer.AllowedSchemes.Add("https");
        _sanitizer.AllowedSchemes.Add("mailto");

        // An "event handler": this code runs for every HTML element the
        // sanitizer keeps, so we can add attributes. "+=" subscribes to the event;
        // "(_, e) => { ... }" is a lambda taking the sender (ignored: "_") and event data.
        _sanitizer.PostProcessNode += (_, e) =>
        {
            // "is not ... element" checks the type and, if it matches,
            // gives us a variable "element" of that type.
            if (e.Node is not AngleSharp.Dom.IElement element)
                return;

            switch (element.TagName)
            {
                case "IMG":
                    // Load images only when they scroll into view (faster pages).
                    element.SetAttribute("loading", "lazy");
                    element.SetAttribute("decoding", "async");
                    break;
                // "when" adds a condition to the case: only links to other websites.
                case "A" when element.GetAttribute("href") is { } href
                              && (href.StartsWith("http://") || href.StartsWith("https://")):
                    // External links open in a new tab, and "noopener" stops the
                    // opened page from getting access to this one.
                    element.SetAttribute("target", "_blank");
                    element.SetAttribute("rel", "noopener noreferrer");
                    break;
            }
        };
    }

    // Converts Markdown to safe HTML.
    // Returns IHtmlContent so Razor views output it as HTML. A normal string
    // would be "HTML-encoded" by Razor, showing the tags as visible text.
    public IHtmlContent Render(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return HtmlString.Empty;

        var html = Markdown.ToHtml(markdown, _pipeline);   // 1. Markdown -> HTML
        return new HtmlString(_sanitizer.Sanitize(html));  // 2. remove anything unsafe
    }
}
