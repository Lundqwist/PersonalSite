// ════════════════════════════════════════════════════════════════════════════
// AppDbContextModelSnapshot.cs — EF Core's picture of what the database
// looks like after ALL migrations.
//
// The EF Core tools ("dotnet ef migrations add ...") compare your models with
// this snapshot to work out what a new migration must change. It is not run
// when the site starts. Keep it in step with the migrations: when a migration
// adds a column, the snapshot must list that column too.
// ════════════════════════════════════════════════════════════════════════════
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PersonalSite.Data;

#nullable disable

namespace PersonalSite.Migrations;

[DbContext(typeof(AppDbContext))]
partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 64);

        MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

        modelBuilder.Entity("PersonalSite.Models.CvEntry", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int");

            MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

            b.Property<string>("Comment")
                .HasMaxLength(2000)
                .HasColumnType("varchar(2000)");

            b.Property<string>("CommentEn")
                .HasMaxLength(2000)
                .HasColumnType("varchar(2000)");

            b.Property<string>("Courses")
                .HasMaxLength(1000)
                .HasColumnType("varchar(1000)");

            b.Property<string>("CoursesEn")
                .HasMaxLength(1000)
                .HasColumnType("varchar(1000)");

            b.Property<string>("DisplayDate")
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            b.Property<DateOnly?>("EndDate")
                .HasColumnType("date");

            b.Property<bool>("IsCurrent")
                .HasColumnType("tinyint(1)");

            b.Property<string>("Location")
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            b.Property<string>("LocationEn")
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            b.Property<string>("Organization")
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            b.Property<string>("OrganizationEn")
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            b.Property<int>("SortOrder")
                .HasColumnType("int");

            b.Property<DateOnly?>("StartDate")
                .HasColumnType("date");

            b.Property<string>("Title")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            b.Property<string>("TitleEn")
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            b.Property<string>("Type")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            b.HasKey("Id");
            b.HasIndex("Type", "SortOrder").HasDatabaseName("IX_CvEntries_Type_SortOrder");
            b.ToTable("CvEntries");
        });

        modelBuilder.Entity("PersonalSite.Models.Post", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int");

            MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

            b.Property<string>("Body")
                .HasColumnType("longtext");

            b.Property<string>("Category")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            b.Property<int?>("CoverImageId")
                .HasColumnType("int");

            b.Property<bool>("IsPublished")
                .HasColumnType("tinyint(1)");

            b.Property<string>("Language")
                .IsRequired()
                .HasMaxLength(2)
                .HasColumnType("varchar(2)");

            b.Property<DateOnly>("PublishedDate")
                .HasColumnType("date");

            b.Property<string>("Summary")
                .HasMaxLength(500)
                .HasColumnType("varchar(500)");

            b.Property<string>("Title")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            b.Property<DateTime>("UpdatedAt")
                .HasColumnType("datetime(6)");

            b.Property<string>("Url")
                .HasMaxLength(500)
                .HasColumnType("varchar(500)");

            b.HasKey("Id");
            b.HasIndex("Category", "PublishedDate");
            b.HasIndex("CoverImageId");
            b.ToTable("Posts");
        });

        modelBuilder.Entity("PersonalSite.Models.MediaImage", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int");

            MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

            b.Property<string>("AltText")
                .HasMaxLength(300)
                .HasColumnType("varchar(300)");

            b.Property<string>("ContentType")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("datetime(6)");

            b.Property<byte[]>("Data")
                .IsRequired()
                .HasColumnType("longblob");

            b.Property<string>("FileName")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            b.Property<int>("Height")
                .HasColumnType("int");

            b.Property<string>("ThumbContentType")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            b.Property<byte[]>("ThumbData")
                .IsRequired()
                .HasColumnType("longblob");

            b.Property<int>("Width")
                .HasColumnType("int");

            b.HasKey("Id");
            b.ToTable("MediaImages");
        });

        modelBuilder.Entity("PersonalSite.Models.Post", b =>
        {
            b.HasOne("PersonalSite.Models.MediaImage", "CoverImage")
                .WithMany()
                .HasForeignKey("CoverImageId")
                .OnDelete(DeleteBehavior.SetNull);

            b.Navigation("CoverImage");
        });

        modelBuilder.Entity("PersonalSite.Models.PageContent", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int");

            MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

            b.Property<string>("Body")
                .HasColumnType("longtext");

            b.Property<string>("BodySv")
                .HasColumnType("longtext");

            b.Property<string>("Key")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            b.Property<string>("Title")
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            b.Property<string>("TitleSv")
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            b.Property<DateTime>("UpdatedAt")
                .HasColumnType("datetime(6)");

            b.HasKey("Id");
            b.HasIndex("Key").IsUnique();
            b.ToTable("PageContents");
        });

        modelBuilder.Entity("PersonalSite.Models.AdminUser", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("int");

            MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

            b.Property<string>("PasswordHash")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            b.Property<string>("UserName")
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            b.HasKey("Id");
            b.HasIndex("UserName").IsUnique();
            b.ToTable("AdminUsers");
        });
#pragma warning restore 612, 618
    }
}
