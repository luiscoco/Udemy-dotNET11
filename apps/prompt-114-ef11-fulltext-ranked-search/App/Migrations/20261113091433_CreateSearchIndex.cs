using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations;

/// <inheritdoc />
public partial class _20261113091433_CreateSearchIndex : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Articles",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Contents = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Articles", x => x.Id);
            });
        // Reviewed provider SQL fallback: EF Core 11.0.0 scaffolding cannot emit
        // its full-text catalog metadata as a C# literal on this installed build.
        migrationBuilder.Sql("CREATE FULLTEXT CATALOG courseFT;", suppressTransaction: true);
        migrationBuilder.Sql("CREATE FULLTEXT INDEX ON Articles(Contents LANGUAGE 1033) KEY INDEX PK_Articles ON courseFT WITH CHANGE_TRACKING AUTO;", suppressTransaction: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP FULLTEXT INDEX ON Articles;", suppressTransaction: true);
        migrationBuilder.Sql("DROP FULLTEXT CATALOG courseFT;", suppressTransaction: true);
        migrationBuilder.DropTable(
            name: "Articles");
    }
}
