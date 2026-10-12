namespace PersonalSite.Models;

// Data for the error page (Views/Shared/Error.cshtml), shown in production
// when something goes wrong (see UseExceptionHandler in Program.cs).
// This class came with the ASP.NET Core project template.
public class ErrorViewModel
{
    // An id identifying the failed request, useful when searching the logs.
    public string? RequestId { get; set; }

    // True when there is an id to show.
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
