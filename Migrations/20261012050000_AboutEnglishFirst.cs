// ════════════════════════════════════════════════════════════════════════════
// MIGRATION 7 of 11 — AboutEnglishFirst: the About page becomes English-first.
// Only renames columns (Title -> TitleSv, TitleEn -> Title, and the same for
// Body), so existing text keeps its language.
// ════════════════════════════════════════════════════════════════════════════
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PersonalSite.Data;

#nullable disable

namespace PersonalSite.Migrations;

// The About page becomes English-first: the Swedish columns are renamed to
// *Sv and the English columns take over Title/Body. Existing text keeps its
// language.
[DbContext(typeof(AppDbContext))]
[Migration("20261012050000_AboutEnglishFirst")]
public partial class AboutEnglishFirst : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(name: "Title", table: "PageContents", newName: "TitleSv");
        migrationBuilder.RenameColumn(name: "Body", table: "PageContents", newName: "BodySv");
        migrationBuilder.RenameColumn(name: "TitleEn", table: "PageContents", newName: "Title");
        migrationBuilder.RenameColumn(name: "BodyEn", table: "PageContents", newName: "Body");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(name: "Title", table: "PageContents", newName: "TitleEn");
        migrationBuilder.RenameColumn(name: "Body", table: "PageContents", newName: "BodyEn");
        migrationBuilder.RenameColumn(name: "TitleSv", table: "PageContents", newName: "Title");
        migrationBuilder.RenameColumn(name: "BodySv", table: "PageContents", newName: "Body");
    }
}
