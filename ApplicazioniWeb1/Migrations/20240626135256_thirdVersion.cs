using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApplicazioniWeb1.Migrations
{
    /// <inheritdoc />
    public partial class thirdVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "TimeSpan",
                table: "Books",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeSpan",
                table: "Books");
        }
    }
}
