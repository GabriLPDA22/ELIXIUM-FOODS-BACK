using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UberEatsBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddRestaurantHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RestaurantHours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DayOfWeek = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsOpen = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    OpenTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    CloseTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    RestaurantId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantHours_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantHour_DayOfWeek",
                table: "RestaurantHours",
                column: "DayOfWeek");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantHour_Restaurant_Day",
                table: "RestaurantHours",
                columns: new[] { "RestaurantId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantHour_RestaurantId",
                table: "RestaurantHours",
                column: "RestaurantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RestaurantHours");
        }
    }
}
