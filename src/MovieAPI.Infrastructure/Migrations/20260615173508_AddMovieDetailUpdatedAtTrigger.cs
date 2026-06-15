using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMovieDetailUpdatedAtTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // RECURSIVE_TRIGGERS is off by default, so this trigger's own UPDATE
            // does not re-fire itself.
            migrationBuilder.Sql(
                """
                CREATE TRIGGER TR_MovieDetails_UpdatedAt ON MovieDetails
                AFTER UPDATE AS
                BEGIN
                    SET NOCOUNT ON;
                    UPDATE md
                    SET UpdatedAt = SYSUTCDATETIME()
                    FROM MovieDetails md
                    INNER JOIN inserted i ON md.Id = i.Id;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER TR_MovieDetails_UpdatedAt");
        }
    }
}
