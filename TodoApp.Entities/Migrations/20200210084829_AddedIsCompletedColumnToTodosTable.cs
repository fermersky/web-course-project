using Microsoft.EntityFrameworkCore.Migrations;

namespace TodoApp.Entities.Migrations
{
    public partial class AddedIsCompletedColumnToTodosTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Todos",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Todos");
        }
    }
}
