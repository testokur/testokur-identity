namespace TestOkur.Identity.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class ActivityLogCreatedByAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "activity_logs",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by",
                table: "activity_logs");
        }
    }
}
