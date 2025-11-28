using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoginApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addpriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "TaskStatus",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "To Do");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Tasks");

            migrationBuilder.UpdateData(
                table: "TaskStatus",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Pending");
        }
    }
}
