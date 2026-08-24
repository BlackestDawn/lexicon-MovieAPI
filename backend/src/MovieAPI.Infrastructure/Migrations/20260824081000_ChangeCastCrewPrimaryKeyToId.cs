using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCastCrewPrimaryKeyToId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CastCrews",
                table: "CastCrews");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CastCrews",
                table: "CastCrews",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_CastCrews_MovieId_PersonId_Role",
                table: "CastCrews",
                columns: new[] { "MovieId", "PersonId", "Role" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CastCrews",
                table: "CastCrews");

            migrationBuilder.DropIndex(
                name: "IX_CastCrews_MovieId_PersonId_Role",
                table: "CastCrews");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CastCrews",
                table: "CastCrews",
                columns: new[] { "MovieId", "PersonId" });
        }
    }
}
