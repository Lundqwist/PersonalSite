// ════════════════════════════════════════════════════════════════════════════
// MIGRATION 1 of 7 — InitialCreate: creates the CvEntries table.
//
// What is a migration?
//   A migration is one step in the history of the database's structure.
//   Up()   = apply the change (create a table, add a column, insert rows...)
//   Down() = undo it again.
//   The [Migration("...")] name starts with a date, which sets the order.
//   The database keeps a list of applied migrations in a table called
//   __EFMigrationsHistory, and db.Database.Migrate() in Program.cs runs the
//   ones not applied yet, every time the site starts.
//
//   Never edit a migration that has already run on the real database —
//   add a new one instead, as the later migrations in this folder do.
//
// This first migration also inserted 9 sample CV rows from the project
// template; migration 2 (RealCvData) replaced them with the real CV.
// ════════════════════════════════════════════════════════════════════════════
using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace PersonalSite.Migrations;

[DbContext(typeof(PersonalSite.Data.AppDbContext))]
[Migration("20240101000000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "CvEntries",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Organization = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Location = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                IsCurrent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                Comment = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                SortOrder = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CvEntries", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.InsertData(
            table: "CvEntries",
            columns: new[] { "Id", "Comment", "EndDate", "IsCurrent", "Location", "Organization", "SortOrder", "StartDate", "Title", "Type" },
            columnTypes: new[] { "int", "varchar(2000)", "date", "tinyint(1)", "varchar(200)", "varchar(200)", "int", "date", "varchar(200)", "varchar(50)" },
            values: new object[,]
            {
                { 1, "Leading the backend architecture for a cloud-native SaaS platform serving 50,000+ customers. Introduced event-driven microservices reducing latency by 40%.", null, true, "Stockholm, Sweden", "Tech Innovations AB", 1, new DateOnly(2021, 3, 1), "Senior Software Engineer", "Workplace" },
                { 2, "Full-stack development with .NET and React. Built internal tooling that automated 60% of the QA pipeline.", new DateOnly(2021, 2, 28), false, "Göteborg, Sweden", "Nordic Solutions", 2, new DateOnly(2018, 6, 1), "Software Developer", "Workplace" },
                { 3, "First professional role. Worked across three products simultaneously in a fast-paced startup environment.", new DateOnly(2018, 5, 31), false, "Malmö, Sweden", "Startup Studio", 3, new DateOnly(2016, 9, 1), "Junior Developer", "Workplace" },
                { 4, "Professional certification covering scalable cloud architecture, security best practices, and cost optimization.", new DateOnly(2022, 4, 1), false, "Online", "Amazon Web Services", 1, new DateOnly(2022, 1, 1), "AWS Solutions Architect", "Course" },
                { 5, "Deep dive into async programming, memory management, and performance tuning in .NET 5.", new DateOnly(2020, 6, 1), false, "Online", "Pluralsight", 2, new DateOnly(2020, 3, 1), "Advanced C# and .NET", "Course" },
                { 6, "Specialisation in distributed systems and machine learning. Thesis on real-time data processing pipelines.", new DateOnly(2016, 6, 1), false, "Stockholm, Sweden", "KTH Royal Institute of Technology", 1, new DateOnly(2014, 9, 1), "M.Sc. Computer Science", "School" },
                { 7, "Core studies in algorithms, databases, and software engineering. Dean's List all three years.", new DateOnly(2014, 6, 1), false, "Lund, Sweden", "Lund University", 2, new DateOnly(2011, 9, 1), "B.Sc. Computer Science", "School" },
                { 8, "Mentoring early-stage founders and organizing quarterly meetups for 200+ members.", null, true, "Stockholm, Sweden", "Swedish Tech Founders Network", 1, new DateOnly(2022, 1, 1), "Board Member", "Board" },
                { 9, "Volunteering to teach programming to children aged 9-13 in underserved communities.", null, true, "Remote", "Code Club Nordic", 2, new DateOnly(2020, 5, 1), "Advisor", "Board" }
            });

        migrationBuilder.CreateIndex(
            name: "IX_CvEntries_Type_SortOrder",
            table: "CvEntries",
            columns: new[] { "Type", "SortOrder" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "CvEntries");
    }
}
