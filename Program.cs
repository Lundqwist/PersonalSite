// ════════════════════════════════════════════════════════════════════════════
// Program.cs — the starting point of the website.
//
// When you run "dotnet run", .NET executes this file from top to bottom.
// It has two phases:
//
//   1. BUILD:  register "services" — reusable objects the app needs, such as
//              the database connection, login handling and the Markdown
//              renderer. (builder.Services.Add...)
//   2. RUN:    set up the "middleware pipeline" — the chain of steps every web
//              request passes through (static files, language, login, ...),
//              then start listening for requests. (app.Use..., app.Run())
//
// This file uses "top-level statements": there is no class or Main method
// written out; the compiler wraps this code in one for you.
// ════════════════════════════════════════════════════════════════════════════

// "using" lines import namespaces, so we can write e.g. "AppDbContext"
// instead of the full name "PersonalSite.Data.AppDbContext".
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonalSite.Data;
using PersonalSite.Models;
using PersonalSite.Services;

// The builder collects configuration (appsettings.json, environment
// variables, command-line arguments) and the list of services.
var builder = WebApplication.CreateBuilder(args);

// ── 1. SERVICES ─────────────────────────────────────────────────────────────
// "Dependency injection" (DI): instead of each class creating its own objects
// with "new", we register them here once. Any controller or view that asks for
// one (for example in its constructor) gets it handed over automatically.

// MVC = Model-View-Controller. Controllers handle requests, Views (.cshtml)
// produce the HTML, Models are the data classes.
builder.Services.AddControllersWithViews(options =>
{
    // Every form POST must include a hidden "antiforgery token". This stops
    // other websites from secretly submitting forms to this site on behalf of
    // a logged-in visitor (an attack called CSRF).
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// The antiforgery cookie is only sent over HTTPS when the site uses HTTPS.
builder.Services.AddAntiforgery(options =>
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest);

// ── Running behind the Synology reverse proxy ──
// On the NAS, visitors connect to DSM's web server (with HTTPS), which passes
// the request on to this app over plain http. The proxy adds headers telling
// us the visitor's real IP address (X-Forwarded-For) and that they used HTTPS
// (X-Forwarded-Proto). UseForwardedHeaders (below) applies them, so that the
// login cookie is marked secure and the login rate limit counts each visitor
// separately. Only headers from the proxy on the same machine (localhost) are
// trusted, which is the default.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// ── Encryption keys for cookies ──
// The login cookie and the forms' antiforgery tokens are encrypted with keys
// that ASP.NET Core creates itself. Inside a container those keys would be
// lost on every restart (logging you out). When "DataProtection:KeysPath" is
// set (on the NAS: an environment variable in docker-compose.yml), the keys are
// saved in that folder instead, so they survive restarts and updates.
var keysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(keysPath))
{
    builder.Services.AddDataProtection()
        .SetApplicationName("PersonalSite")
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath));
}

// The database. Entity Framework Core (EF Core) is an "ORM": it lets us work
// with C# objects and translates that into SQL for us. "Pomelo" is the
// package that teaches EF Core to talk to MySQL/MariaDB (the database on the
// Synology NAS). The connection string is read from appsettings.json.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Lifetimes: "Singleton" = one shared instance for the whole app (fine here,
// because MarkdownRenderer keeps no per-visitor data). "Scoped" = one new
// instance per web request (the database context is scoped).
builder.Services.AddSingleton<MarkdownRenderer>();

// ── Admin login ──
// PasswordHasher turns a password into a salted, slow-to-crack hash
// (PBKDF2). We never store the real password, only this hash.
builder.Services.AddScoped<IPasswordHasher<AdminUser>, PasswordHasher<AdminUser>>();

// Cookie authentication: after a successful login the server gives the
// browser an encrypted cookie. The browser sends it with every request,
// which is how the site remembers that you are logged in.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Account/Login";        // where to send visitors who are not logged in
        options.LogoutPath = "/Admin/Account/Logout";
        options.AccessDeniedPath = "/Admin/Account/Login";
        options.Cookie.Name = "PersonalSite.Admin";
        options.Cookie.HttpOnly = true;                    // JavaScript cannot read the cookie
        options.Cookie.SameSite = SameSiteMode.Strict;     // not sent along from other websites
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS-only when the site uses HTTPS
        options.ExpireTimeSpan = TimeSpan.FromHours(8);    // log out after 8 hours...
        options.SlidingExpiration = true;                  // ...of inactivity (each visit extends it)
    });
builder.Services.AddAuthorization();

// Rate limiting slows down anyone trying to guess the password:
// at most 5 login attempts per minute from the same IP address.
// Extra attempts get HTTP status 429 "Too Many Requests".
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    // "login" is the policy name; the login action opts in with [EnableRateLimiting("login")].
    // "?." and "??" handle null: if there is no IP address, use "unknown".
    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1) }));
});

// Build() freezes the service list and creates the app.
var app = builder.Build();

// ── Database migrations ─────────────────────────────────────────────────────
// A "migration" is a recorded change to the database structure (create a
// table, add a column, ...), stored as C# files in the Migrations folder.
// Migrate() applies any migrations the database has not seen yet, so the
// database is always up to date when the site starts.
// "using" with braces makes sure the scope (and the database connection
// inside it) is cleaned up afterwards.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ── 2. MIDDLEWARE PIPELINE ──────────────────────────────────────────────────
// Each "app.Use..." adds a step. A request passes through them in this
// order, so the order matters (e.g. authentication before authorization).

// First: read the reverse proxy's headers (see ForwardedHeadersOptions above),
// so every later step sees the visitor's real IP address and HTTPS.
app.UseForwardedHeaders();

// In production, show a friendly error page instead of technical details,
// and tell browsers to always use HTTPS (HSTS).
// "Development" is set in Properties/launchSettings.json when you run locally.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();   // redirect http:// to https:// when HTTPS is available
app.UseStaticFiles();        // serve files from wwwroot (CSS, JavaScript, images) directly

// Language: Swedish (sv-SE) is the default. A visitor gets English when they
// pick it with the language switch (saved in a cookie by LanguageController)
// or when their browser asks for English. This sets CultureInfo.CurrentCulture,
// which Lang.cs reads, and which also controls date formats ("aug." vs "Aug").
var supportedCultures = new[] { "sv-SE", "en-US" };
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures));

app.UseRouting();            // work out which controller/action should handle the URL
app.UseRateLimiter();        // apply the "login" limit defined above
app.UseAuthentication();     // read the login cookie: who is this visitor?
app.UseAuthorization();      // check [Authorize]: is this visitor allowed here?

// ── Routes: how URLs map to controllers ─────────────────────────────────────
// Admin pages live in an "Area" called Admin: /Admin/{controller}/{action}/{id}.
// "/Admin" alone goes to DashboardController.Index.
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Public pages: /{controller}/{action}/{id}. Examples:
//   /                    -> HomeController.Index
//   /Stories             -> StoriesController.Index
//   /Stories/Details/7   -> StoriesController.Details(id: 7)
// "{id?}" means the id part is optional.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Start the web server and wait for requests (runs until you stop it).
app.Run();
