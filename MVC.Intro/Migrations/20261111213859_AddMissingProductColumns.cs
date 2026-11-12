using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GerchevCarting.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingProductColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The Products table was created by an older EnsureCreated() snapshot.
            // Notes already exists; only FragranceFamily was added later and is missing.
            migrationBuilder.Sql(@"ALTER TABLE ""Products"" ADD COLUMN ""FragranceFamily"" TEXT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // SQLite does not support DROP COLUMN — no-op.
        }
    }
}
