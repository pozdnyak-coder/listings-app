using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ListingsApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Listings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Rooms = table.Column<int>(type: "integer", nullable: false),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PhotoUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DistrictId = table.Column<int>(type: "integer", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Listings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Listings_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Listings_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Districts",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Чиланзар" },
                    { 2, "Юнусабад" },
                    { 3, "Мирзо-Улугбек" },
                    { 4, "Яккасарай" },
                    { 5, "Сергели" },
                    { 6, "Шайхантахур" },
                    { 7, "Мирабад" },
                    { 8, "Алмазар" }
                });

            migrationBuilder.InsertData(
                table: "Listings",
                columns: new[] { "Id", "Address", "CreatedAt", "DistrictId", "OwnerId", "PhotoUrl", "Price", "Rooms", "Title" },
                values: new object[,]
                {
                    { 1, "Чиланзар, 12 кв., д. 4", new DateTime(2026, 9, 1, 9, 0, 0, 0, DateTimeKind.Utc), 1, null, null, 48000m, 2, "2-комн. рядом с метро Чиланзар" },
                    { 2, null, new DateTime(2026, 9, 5, 10, 0, 0, 0, DateTimeKind.Utc), 2, null, null, 71000m, 3, "3-комн. с ремонтом" },
                    { 3, null, new DateTime(2026, 9, 8, 14, 0, 0, 0, DateTimeKind.Utc), 3, null, null, 32000m, 1, "1-комн. студия" },
                    { 4, "Юнусабад, 4 кв.", new DateTime(2026, 9, 12, 8, 0, 0, 0, DateTimeKind.Utc), 2, null, null, 95000m, 4, "4-комн. с видом на парк" },
                    { 5, null, new DateTime(2026, 9, 15, 11, 0, 0, 0, DateTimeKind.Utc), 1, null, null, 52000m, 2, "2-комн. после ремонта, тихий двор" },
                    { 6, null, new DateTime(2026, 9, 18, 16, 0, 0, 0, DateTimeKind.Utc), 2, null, null, 36000m, 1, "1-комн. у ТРЦ" },
                    { 7, null, new DateTime(2026, 9, 20, 9, 0, 0, 0, DateTimeKind.Utc), 3, null, null, 66000m, 3, "3-комн. семейная" },
                    { 8, null, new DateTime(2026, 9, 24, 12, 0, 0, 0, DateTimeKind.Utc), 1, null, null, 39000m, 2, "2-комн. срочно, торг" },
                    { 9, null, new DateTime(2026, 9, 25, 10, 0, 0, 0, DateTimeKind.Utc), 4, null, null, 58000m, 2, "2-комн. Яккасарай, центр" },
                    { 10, null, new DateTime(2026, 9, 26, 13, 0, 0, 0, DateTimeKind.Utc), 5, null, null, 29000m, 1, "1-комн. Сергели, новостройка" },
                    { 11, null, new DateTime(2026, 9, 27, 15, 0, 0, 0, DateTimeKind.Utc), 6, null, null, 74000m, 3, "3-комн. Шайхантахур у метро" },
                    { 12, null, new DateTime(2026, 9, 28, 9, 0, 0, 0, DateTimeKind.Utc), 7, null, null, 61000m, 2, "2-комн. Мирабад, евроремонт" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Districts_Name",
                table: "Districts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Listings_DistrictId",
                table: "Listings",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Listings_OwnerId",
                table: "Listings",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Listings");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
