using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGenreSlugIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Genres_Slug",
                table: "Genres",
                column: "Slug");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Genres_Slug",
                table: "Genres");
        }
    }
}
