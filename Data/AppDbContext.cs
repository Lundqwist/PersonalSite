// ════════════════════════════════════════════════════════════════════════════
// Data/AppDbContext.cs — the connection between the C# models and the database.
//
// A "DbContext" is EF Core's main class. It knows which tables exist (the
// DbSet properties) and how each model maps to a table (OnModelCreating).
// Controllers get one per request through dependency injection
// (registered with AddDbContext in Program.cs).
//
// Typical use in a controller:
//     var posts = await _db.Posts.Where(p => p.IsPublished).ToListAsync();  // SELECT
//     _db.Posts.Add(post); await _db.SaveChangesAsync();                     // INSERT
// ════════════════════════════════════════════════════════════════════════════

using Microsoft.EntityFrameworkCore;
using PersonalSite.Models;

namespace PersonalSite.Data;

// ": DbContext" means AppDbContext inherits from (extends) EF Core's DbContext.
public class AppDbContext : DbContext
{
    // The constructor receives the settings (which database, connection
    // string) from Program.cs and passes them on to the base class.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Each DbSet is a table you can query with LINQ (C#'s query syntax).
    public DbSet<CvEntry> CvEntries => Set<CvEntry>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<PageContent> PageContents => Set<PageContent>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<MediaImage> MediaImages => Set<MediaImage>();

    // Extra mapping rules that cannot be expressed with attributes on the models.
    // "protected override" replaces the base class's (empty) version of this method.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // CV content is edited on the admin pages; the initial rows are
        // inserted by migrations from CvSeedData.
        // "e => { ... }" is a lambda: a small inline function that receives
        // a builder ("e") for the CvEntry table.
        modelBuilder.Entity<CvEntry>(e =>
        {
            e.HasKey(x => x.Id);
            // Store the enum as readable text ("Course") instead of a number.
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
            // An index makes "sort by Type, then SortOrder" fast.
            e.HasIndex(x => new { x.Type, x.SortOrder });
        });

        modelBuilder.Entity<Post>(e =>
        {
            e.Property(x => x.Category).HasConversion<string>().HasMaxLength(50);
            e.HasIndex(x => new { x.Category, x.PublishedDate });

            // Relationship: a post may have one cover image; an image can be
            // the cover of many posts. When an image is deleted, the posts that
            // used it simply lose their cover (CoverImageId is set to NULL)
            // instead of being deleted too.
            e.HasOne(x => x.CoverImage)
                .WithMany()
                .HasForeignKey(x => x.CoverImageId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Unique index: the database refuses two rows with the same Key.
        modelBuilder.Entity<PageContent>(e =>
        {
            e.HasIndex(x => x.Key).IsUnique();
        });

        // Two admin users can't have the same username.
        modelBuilder.Entity<AdminUser>(e =>
        {
            e.HasIndex(x => x.UserName).IsUnique();
        });
    }
}
