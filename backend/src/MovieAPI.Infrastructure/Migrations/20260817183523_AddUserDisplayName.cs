using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDisplayName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Added nullable first so existing rows (e.g. a seeded admin account) can be
            // backfilled from their email's local part - the same fallback the
            // application uses for new accounts that don't supply a DisplayName - before
            // the column is locked down to NOT NULL.
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE AspNetUsers
                SET DisplayName = LEFT(Email, CHARINDEX('@', Email) - 1)
                WHERE DisplayName IS NULL AND Email IS NOT NULL AND CHARINDEX('@', Email) > 0
            ");

            migrationBuilder.Sql(@"
                UPDATE AspNetUsers
                SET DisplayName = 'User'
                WHERE DisplayName IS NULL
            ");

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "AspNetUsers");
        }
    }
}
