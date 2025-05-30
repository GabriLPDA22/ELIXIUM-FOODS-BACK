using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UberEatsBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddProductOffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderItemOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderItemId = table.Column<int>(type: "integer", nullable: false),
                    OfferId = table.Column<int>(type: "integer", nullable: false),
                    OfferName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DiscountType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    OriginalPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    FinalPrice = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItemOffers_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DiscountType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "percentage"),
                    DiscountValue = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    MinimumOrderAmount = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0m),
                    MinimumQuantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UsageLimit = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    UsageCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "active"),
                    RestaurantId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductOffers_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductOffers_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemOffer_AppliedAt",
                table: "OrderItemOffers",
                column: "AppliedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemOffer_OfferId",
                table: "OrderItemOffers",
                column: "OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemOffer_OrderItemId",
                table: "OrderItemOffers",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOffer_ActiveOffers",
                table: "ProductOffers",
                columns: new[] { "Status", "StartDate", "EndDate", "RestaurantId" },
                filter: "\"Status\" = 'active'");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOffer_DateRange",
                table: "ProductOffers",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductOffer_ProductId",
                table: "ProductOffers",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOffer_Restaurant_Product",
                table: "ProductOffers",
                columns: new[] { "RestaurantId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductOffer_RestaurantId",
                table: "ProductOffers",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOffer_Status",
                table: "ProductOffers",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItemOffers");

            migrationBuilder.DropTable(
                name: "ProductOffers");
        }
    }
}
