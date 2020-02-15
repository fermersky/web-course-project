using Microsoft.EntityFrameworkCore.Migrations;

namespace TodoApp.Entities.Migrations
{
    public partial class RenameFileFieldColumnToFileUrlInTodosTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Todos");

            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "Todos",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "Todos");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Todos",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
