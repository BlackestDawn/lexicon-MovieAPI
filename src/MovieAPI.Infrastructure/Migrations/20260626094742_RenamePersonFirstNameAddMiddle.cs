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
            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Persons",
                newName: "GivenName");

            migrationBuilder.RenameIndex(
                name: "IX_Persons_FirstName",
                table: "Persons",
                newName: "IX_Persons_GivenName");

            migrationBuilder.AddColumn<string>(
                name: "MiddleName",
                table: "Persons",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_MiddleName",
                table: "Persons",
                column: "MiddleName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Persons_MiddleName",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "MiddleName",
                table: "Persons");

            migrationBuilder.RenameColumn(
                name: "GivenName",
                table: "Persons",
                newName: "FirstName");

            migrationBuilder.RenameIndex(
                name: "IX_Persons_GivenName",
                table: "Persons",
                newName: "IX_Persons_FirstName");
        }
    }
}
