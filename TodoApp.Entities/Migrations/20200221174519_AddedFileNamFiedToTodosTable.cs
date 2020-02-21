using Microsoft.EntityFrameworkCore.Migrations;

namespace TodoApp.Entities.Migrations
{
    public partial class AddedFileNamFiedToTodosTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Todos",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Todos");
        }
    }
}
