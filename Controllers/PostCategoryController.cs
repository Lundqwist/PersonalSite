// ════════════════════════════════════════════════════════════════════════════
// Controllers/PostCategoryController.cs — Stories, Projects and Gaming.
//
// The three pages work exactly the same, they just show different posts.
// So the logic is written once in an "abstract base class", and three tiny
// classes inherit from it, each only saying which category it shows:
//
//   /Stories              -> StoriesController.Index    (list)
//   /Stories/Details/7    -> StoriesController.Details  (one post)
//   /Projects, /Gaming    -> the same, for their category
//
// All three use the shared views Views/Shared/PostList.cshtml and PostDetails.cshtml.
// ════════════════════════════════════════════════════════════════════════════

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalSite.Data;
using PersonalSite.Models;

namespace PersonalSite.Controllers;

// "abstract" = this class can't be used on its own, only inherited from.
// (So ASP.NET Core does not create a /PostCategory page.)
public abstract class PostCategoryController : Controller
{
    private readonly AppDbContext _db;

    // "protected" = only accessible from this class and classes inheriting from it.
    protected PostCategoryController(AppDbContext db)
    {
        _db = db;
    }

    // Each subclass MUST say which category it shows ("abstract" property).
    protected abstract PostCategory Category { get; }

    // The list page: published posts of this category, newest first.
    public async Task<IActionResult> Index()
    {
        var posts = await _db.Posts
            .Where(p => p.Category == Category && p.IsPublished)
            .OrderByDescending(p => p.PublishedDate)
            .ToListAsync();

        // ViewData is a small dictionary for passing extra values to a view.
        // The view uses the category to pick the heading and header image.
        ViewData["Category"] = Category;

        // View("PostList", posts): use the view named PostList (found in
        // Views/Shared because there is no Views/Stories/PostList.cshtml).
        return View("PostList", posts);
    }

    // One post. "id" comes from the URL: /Stories/Details/7 -> id = 7.
    public async Task<IActionResult> Details(int id)
    {
        // Also check the category and IsPublished, so drafts and posts from
        // another category can't be opened by guessing numbers.
        var post = await _db.Posts
            .FirstOrDefaultAsync(p => p.Id == id && p.Category == Category && p.IsPublished);

        return post == null ? NotFound() : View("PostDetails", post);
    }
}

// The three real controllers. ": base(db)" passes the database on to the
// base class constructor. "override" provides the abstract Category.

public class StoriesController : PostCategoryController
{
    public StoriesController(AppDbContext db) : base(db) { }
    protected override PostCategory Category => PostCategory.Story;
}

public class ProjectsController : PostCategoryController
{
    public ProjectsController(AppDbContext db) : base(db) { }
    protected override PostCategory Category => PostCategory.Project;
}

public class GamingController : PostCategoryController
{
    public GamingController(AppDbContext db) : base(db) { }
    protected override PostCategory Category => PostCategory.Gaming;
}
