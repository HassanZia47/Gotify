using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Goatify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedByAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Medications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Goats",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Expenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Breedings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Bills",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Medications");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Goats");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Breedings");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Bills");
        }
    }
}
