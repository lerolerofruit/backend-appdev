using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IMS_API_.Migrations.IMS
{
    /// <inheritdoc />
    public partial class AddServiceNotesToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ServiceNotes",
                table: "ServiceAppointments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceNotes",
                table: "ServiceAppointments");
        }
    }
}
