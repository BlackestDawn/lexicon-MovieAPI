using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenamePersonFirstNameAddMiddle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Persons_FirstName",
                table: "Persons");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Persons",
                newName: "GivenName");

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "Persons",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_GivenName",
                table: "Persons",
                column: "GivenName");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_MiddleName",
                table: "Persons",
                column: "GivenName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Persons_GivenName",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "GivenName",
                table: "Persons");

            migrationBuilder.RenameColumn(
                name: "MiddleName",
                table: "Persons",
                newName: "FirstName");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_FirstName",
                table: "Persons",
                column: "FirstName");
        }
    }
}
