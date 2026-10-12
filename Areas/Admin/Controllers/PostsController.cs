// ════════════════════════════════════════════════════════════════════════════
// Areas/Admin/Controllers/PostsController.cs — admin for Stories, Projects and Portfolio.
//
// One controller handles all three; "category" in the URL says which:
//   GET  /Admin/Posts?category=Story              list of stories
//   GET  /Admin/Posts/Create?category=Project     new-project form   (POST: save)
//   GET  /Admin/Posts/Edit/7                      edit post 7        (POST: save)
//   POST /Admin/Posts/Delete/7                    delete post 7
//   POST /Admin/Posts/Preview                     Markdown preview (called by JavaScript)
// ════════════════════════════════════════════════════════════════════════════

using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalSite.Areas.Admin.Models;
using PersonalSite.Data;
using PersonalSite.Models;
using PersonalSite.Services;

namespace PersonalSite.Areas.Admin.Controllers;

public class PostsController : AdminControllerBase
{
    // Fields the edit form is allowed to change (see the explanation in
    // Admin/CvController.cs). Category is NOT here: it is set when the post is
    // created and can't be changed by posting a different value.
    private static readonly Expression<Func<Post, object?>>[] EditableFields =
    {
        p => p.Language,
        p => p.Title, p => p.Summary, p => p.Body,
        p => p.CoverImageId,
        p => p.Url, p => p.PublishedDate, p => p.IsPublished,
    };

    private readonly AppDbContext _db;
    private readonly MarkdownRenderer _markdown;

    public PostsController(AppDbContext db, MarkdownRenderer markdown)
    {
        _db = db;
        _markdown = markdown;
    }

    // The list for one category. ASP.NET Core converts "?category=Story" to
    // the enum value PostCategory.Story automatically.
    public async Task<IActionResult> Index(PostCategory category = PostCategory.Story)
    {
        var posts = await _db.Posts
            .Where(p => p.Category == category)
            .OrderByDescending(p => p.PublishedDate)
            .ThenByDescending(p => p.Id)
            .ToListAsync();

        ViewData["Category"] = category;
        return View(posts);
    }

    [HttpGet]
    public async Task<IActionResult> Create(PostCategory category = PostCategory.Story)
    {
        return await EditView(new Post { Category = category });
    }

    // The POST version gets a different C# name (CreatePost) but the same
    // URL thanks to [ActionName("Create")].
    [HttpPost]
    [ActionName("Create")]
    public async Task<IActionResult> CreatePost(PostCategory category)
    {
        var post = new Post { Category = category };
        if (!await TryUpdatePostAsync(post))
            return await EditView(post);

        post.UpdatedAt = DateTime.UtcNow;
        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        SetMessage($"Added \"{post.Title}\".");
        // "new { category }" builds the query string ?category=Story.
        return RedirectToAction(nameof(Index), new { category });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var post = await _db.Posts.FindAsync(id);
        return post == null ? NotFound() : await EditView(post);
    }

    [HttpPost]
    [ActionName("Edit")]
    public async Task<IActionResult> EditPost(int id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post == null)
            return NotFound();

        if (!await TryUpdatePostAsync(post))
            return await EditView(post);

        post.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        SetMessage($"Saved \"{post.Title}\".");
        return RedirectToAction(nameof(Index), new { category = post.Category });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post == null)
            return RedirectToAction(nameof(Index));

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();

        SetMessage($"Deleted \"{post.Title}\".");
        return RedirectToAction(nameof(Index), new { category = post.Category });
    }

    // The editor's "Preview" tab sends the Markdown text here (with fetch() in
    // admin-editor.js) and shows the HTML we return. Content(...) returns
    // plain text/HTML instead of a whole page. The About editor uses it too.
    [HttpPost]
    public IActionResult Preview(string? markdown)
    {
        return Content(_markdown.Render(markdown).ToString() ?? string.Empty, "text/html");
    }

    // Copies the form into the post and validates it, plus one extra check:
    // the chosen cover image must still exist (it could have been deleted in
    // another browser tab). "private" = only used inside this class.
    private async Task<bool> TryUpdatePostAsync(Post post)
    {
        var valid = await TryUpdateModelAsync(post, string.Empty, EditableFields);

        // AnyAsync = "does at least one row match?" -> SELECT EXISTS(...)
        if (post.CoverImageId.HasValue && !await _db.MediaImages.AnyAsync(m => m.Id == post.CoverImageId))
        {
            ModelState.AddModelError(nameof(Post.CoverImageId), "The selected cover image no longer exists.");
            valid = false;
        }

        return valid;
    }

    // Shows the edit form, together with the image library
    // (only names and ids — not the image data itself, which would be slow).
    private async Task<IActionResult> EditView(Post post)
    {
        ViewData["Images"] = await MediaListItem.LoadAllAsync(_db);

        return View("Edit", post);
    }
}
