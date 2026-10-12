using System.ComponentModel.DataAnnotations;

namespace PersonalSite.Models;

// A user allowed to log in to the admin pages (currently only "lunarAdmin").
//
// The password is NEVER stored as plain text. PasswordHash holds the result of
// ASP.NET Core Identity's PasswordHasher: the password mixed with a random
// "salt" and run through PBKDF2 many thousands of times. To check a login we
// hash the typed password the same way and compare (see AccountController).
// Even someone who reads the database cannot get the password back from it.
public class AdminUser
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string UserName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string PasswordHash { get; set; } = string.Empty;
}
