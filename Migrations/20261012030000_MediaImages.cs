// ════════════════════════════════════════════════════════════════════════════
// MIGRATION 5 of 8 — MediaImages: creates the table for uploaded images and
// adds Posts.CoverImageId, a "foreign key" pointing to an image. If that image
// is deleted, the post's CoverImageId is set to NULL (ReferentialAction.SetNull).
// ════════════════════════════════════════════════════════════════════════════
using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using PersonalSite.Data;

#nullable disable

namespace PersonalSite.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261012030000_MediaImages")]
public partial class MediaImages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "MediaImages",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                FileName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                AltText = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ContentType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Width = table.Column<int>(type: "int", nullable: false),
                Height = table.Column<int>(type: "int", nullable: false),
                Data = table.Column<byte[]>(type: "longblob", nullable: false),
                ThumbContentType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ThumbData = table.Column<byte[]>(type: "longblob", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MediaImages", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<int>(
            name: "CoverImageId",
            table: "Posts",
            type: "int",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Posts_CoverImageId",
            table: "Posts",
            column: "CoverImageId");

        migrationBuilder.AddForeignKey(
            name: "FK_Posts_MediaImages_CoverImageId",
            table: "Posts",
            column: "CoverImageId",
            principalTable: "MediaImages",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Posts_MediaImages_CoverImageId",
            table: "Posts");

        migrationBuilder.DropIndex(
            name: "IX_Posts_CoverImageId",
            table: "Posts");

        migrationBuilder.DropColumn(
            name: "CoverImageId",
            table: "Posts");

        migrationBuilder.DropTable(name: "MediaImages");
    }
}
