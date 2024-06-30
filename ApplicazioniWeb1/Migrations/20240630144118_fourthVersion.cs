using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApplicazioniWeb1.Migrations
{
    /// <inheritdoc />
    public partial class fourthVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Invoices",
                table: "Invoices");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Invoices",
                newName: "DateEnd");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateStart",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<float>(
                name: "Value",
                table: "Invoices",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Invoices",
                table: "Invoices",
                columns: new[] { "DateStart", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Invoices",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DateStart",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "Invoices");

            migrationBuilder.RenameColumn(
                name: "DateEnd",
                table: "Invoices",
                newName: "Date");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Invoices",
                table: "Invoices",
                columns: new[] { "Date", "UserId" });
        }
    }
}
