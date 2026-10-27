// ════════════════════════════════════════════════════════════════════════════
// MIGRATION 4 of 9 — AdminAndPosts: creates the tables Posts (stories,
// projects, portfolio), PageContents (the About page) and AdminUsers, and
// adds the admin account "lunarAdmin".
//
// Note: only the HASH of the admin password is stored here, never the
// password itself (see Models/AdminUser.cs). Change the password on the
// admin page "Change password".
// ════════════════════════════════════════════════════════════════════════════
using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using PersonalSite.Data;

#nullable disable

namespace PersonalSite.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261012020000_AdminAndPosts")]
public partial class AdminAndPosts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Posts",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Category = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                TitleEn = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Summary = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                SummaryEn = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Body = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                BodyEn = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                PublishedDate = table.Column<DateOnly>(type: "date", nullable: false),
                IsPublished = table.Column<bool>(type: "tinyint(1)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Posts", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_Posts_Category_PublishedDate",
            table: "Posts",
            columns: new[] { "Category", "PublishedDate" });

        migrationBuilder.CreateTable(
            name: "PageContents",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Key = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                TitleEn = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Body = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                BodyEn = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PageContents", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_PageContents_Key",
            table: "PageContents",
            column: "Key",
            unique: true);

        migrationBuilder.CreateTable(
            name: "AdminUsers",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                UserName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                PasswordHash = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AdminUsers", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_AdminUsers_UserName",
            table: "AdminUsers",
            column: "UserName",
            unique: true);

        // Initial admin account for a NEW installation. Only the PBKDF2 hash of
        // the password is stored. The first-login password is
        //     ChangeMe-OnFirstLogin!
        // Change it immediately on the admin "Change password" page.
        // (An existing database already ran this migration, so editing this
        // line never changes the password of a running site.)
        migrationBuilder.InsertData(
            table: "AdminUsers",
            columns: new[] { "Id", "UserName", "PasswordHash" },
            columnTypes: new[] { "int", "varchar(100)", "varchar(200)" },
            values: new object[] { 1, "lunarAdmin", "AQAAAAIAAYagAAAAELLQT/DXh1mslvJ62PQYe3LZGQegkM2tpKMv7lceAiVFOnXuQySJBEZJIBiFgohDQg==" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AdminUsers");
        migrationBuilder.DropTable(name: "PageContents");
        migrationBuilder.DropTable(name: "Posts");
    }
}
