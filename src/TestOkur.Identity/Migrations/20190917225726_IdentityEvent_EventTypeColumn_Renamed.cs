namespace TestOkur.Identity.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public partial class IdentityEvent_EventTypeColumn_Renamed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "even_type",
                table: "identity_events",
                newName: "event_type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "event_type",
                table: "identity_events",
                newName: "even_type");
        }
    }
}
