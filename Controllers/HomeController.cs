// ════════════════════════════════════════════════════════════════════════════
// Controllers/HomeController.cs — the front page and the error page.
//
// How controllers work (the "C" in MVC):
//   • A controller is a class whose name ends in "Controller".
//   • Each public method is an "action" that handles one kind of request.
//   • The URL decides which one runs (see the routes in Program.cs):
//       /            -> HomeController.Index()
//       /Home/Error  -> HomeController.Error()
//   • The action returns a result, usually View(): "render the matching
//     .cshtml file", here Views/Home/Index.cshtml.
// ════════════════════════════════════════════════════════════════════════════

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PersonalSite.Models;

namespace PersonalSite.Controllers;

// ": Controller" inherits helper methods such as View(), Redirect() and NotFound().
public class HomeController : Controller
{
    // The front page has no data from the database; it just shows the view.
    // IActionResult = "some kind of response" (a page, a redirect, an error...).
    public IActionResult Index() => View();

    // The friendly error page used on the live site (UseExceptionHandler in
    // Program.cs). [ResponseCache] makes sure browsers never cache an error.
    // It shows an id for the failed request, to find it in the server logs.
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
