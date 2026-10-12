// ════════════════════════════════════════════════════════════════════════════
// Areas/Admin/Models/AccountViewModels.cs — the data in the login and
// change-password forms.
//
// A "view model" is a class shaped for one form, rather than for a database
// table. The attributes do two jobs:
//   • validation on the server (ModelState.IsValid in AccountController), and
//   • labels and input types in the form (asp-for in the .cshtml views,
//     e.g. [DataType(DataType.Password)] makes <input type="password">).
// ════════════════════════════════════════════════════════════════════════════

using System.ComponentModel.DataAnnotations;

namespace PersonalSite.Areas.Admin.Models;

public class LoginViewModel
{
    // [Display(Name = ...)] is the label text shown next to the field.
    [Required, Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    // The admin page to return to after logging in (hidden field in the form).
    public string? ReturnUrl { get; set; }
}

public class ChangePasswordViewModel
{
    [Required, DataType(DataType.Password), Display(Name = "Current password")]
    public string CurrentPassword { get; set; } = string.Empty;

    // At least 12 characters long.
    [Required, DataType(DataType.Password), Display(Name = "New password")]
    [StringLength(200, MinimumLength = 12, ErrorMessage = "The new password must be at least 12 characters.")]
    public string NewPassword { get; set; } = string.Empty;

    // [Compare] checks that this field equals NewPassword (catches typos).
    [Required, DataType(DataType.Password), Display(Name = "Confirm new password")]
    [Compare(nameof(NewPassword), ErrorMessage = "The passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
