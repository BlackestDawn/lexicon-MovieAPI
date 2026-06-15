using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedAtTriggers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // RECURSIVE_TRIGGERS is off by default, so each trigger's own UPDATE
            // does not re-fire itself.
            migrationBuilder.Sql(
                """
                CREATE TRIGGER TR_Genres_UpdatedAt ON Genres
                AFTER UPDATE AS
                BEGIN
                    SET NOCOUNT ON;
                    UPDATE g
                    SET UpdatedAt = SYSUTCDATETIME()
                    FROM Genres g
                    INNER JOIN inserted i ON g.Id = i.Id;
                END
                """);

            migrationBuilder.Sql(
                """
                CREATE TRIGGER TR_Movies_UpdatedAt ON Movies
                AFTER UPDATE AS
                BEGIN
                    SET NOCOUNT ON;
                    UPDATE m
                    SET UpdatedAt = SYSUTCDATETIME()
                    FROM Movies m
                    INNER JOIN inserted i ON m.Id = i.Id;
                END
                """);

            migrationBuilder.Sql(
                """
                CREATE TRIGGER TR_Persons_UpdatedAt ON Persons
                AFTER UPDATE AS
                BEGIN
                    SET NOCOUNT ON;
                    UPDATE p
                    SET UpdatedAt = SYSUTCDATETIME()
                    FROM Persons p
                    INNER JOIN inserted i ON p.Id = i.Id;
                END
                """);

            migrationBuilder.Sql(
                """
                CREATE TRIGGER TR_Reviews_UpdatedAt ON Reviews
                AFTER UPDATE AS
                BEGIN
                    SET NOCOUNT ON;
                    UPDATE r
                    SET UpdatedAt = SYSUTCDATETIME()
                    FROM Reviews r
                    INNER JOIN inserted i ON r.Id = i.Id;
                END
                """);

            migrationBuilder.Sql(
                """
                CREATE TRIGGER TR_MovieGenres_UpdatedAt ON MovieGenres
                AFTER UPDATE AS
                BEGIN
                    SET NOCOUNT ON;
                    UPDATE mg
                    SET UpdatedAt = SYSUTCDATETIME()
                    FROM MovieGenres mg
                    INNER JOIN inserted i ON mg.MovieId = i.MovieId AND mg.GenreId = i.GenreId;
                END
                """);

            migrationBuilder.Sql(
                """
                CREATE TRIGGER TR_CastCrews_UpdatedAt ON CastCrews
                AFTER UPDATE AS
                BEGIN
                    SET NOCOUNT ON;
                    UPDATE cc
                    SET UpdatedAt = SYSUTCDATETIME()
                    FROM CastCrews cc
                    INNER JOIN inserted i ON cc.MovieId = i.MovieId AND cc.PersonId = i.PersonId;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER TR_Genres_UpdatedAt");
            migrationBuilder.Sql("DROP TRIGGER TR_Movies_UpdatedAt");
            migrationBuilder.Sql("DROP TRIGGER TR_Persons_UpdatedAt");
            migrationBuilder.Sql("DROP TRIGGER TR_Reviews_UpdatedAt");
            migrationBuilder.Sql("DROP TRIGGER TR_MovieGenres_UpdatedAt");
            migrationBuilder.Sql("DROP TRIGGER TR_CastCrews_UpdatedAt");
        }
    }
}
