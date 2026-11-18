using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations;

/// <inheritdoc />
public partial class _20261113000200_AddSeats : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "Seats",
            table: "Courses",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Seats",
            table: "Courses");
    }
}

