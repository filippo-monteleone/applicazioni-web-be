using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApplicazioniWeb1.Migrations
{
    /// <inheritdoc />
    public partial class ninth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimeSpan",
                table: "Books",
                newName: "TimePark");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "Books",
                newName: "TimeCharge");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimePark",
                table: "Books",
                newName: "TimeSpan");

            migrationBuilder.RenameColumn(
                name: "TimeCharge",
                table: "Books",
                newName: "Time");
        }
    }
}
