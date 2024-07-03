using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApplicazioniWeb1.Migrations
{
    /// <inheritdoc />
    public partial class fifth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "EndValue",
                table: "Invoices",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "StartValue",
                table: "Invoices",
                type: "real",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndValue",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "StartValue",
                table: "Invoices");
        }
    }
}
