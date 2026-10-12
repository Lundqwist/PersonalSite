using System.Globalization;

namespace PersonalSite;

// Small helper for the site's two languages (Swedish is the default).
//
// "static class" means you never create an object of it; you call its
// members directly, e.g. Lang.T("Hej", "Hello") in a view.
//
// Which language is active is decided per request by the localization step
// in Program.cs, which sets CultureInfo.CurrentUICulture.
public static class Lang
{
    // "=>" defines a property whose value is calculated each time it is read.
    // TwoLetterISOLanguageName is "sv" for sv-SE and "en" for en-US.
    public static bool IsEnglish => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en";

    // Language code for the <html lang="..."> attribute, which helps screen
    // readers and search engines.
    public static string Code => IsEnglish ? "en" : "sv";

    // Picks the Swedish or the English text, e.g. Lang.T("Läs mer", "Read more").
    // "condition ? a : b" is the conditional operator: a if true, otherwise b.
    public static string T(string sv, string en) => IsEnglish ? en : sv;
}
