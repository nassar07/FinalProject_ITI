using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace FinalProject_ITI.Migrations
{
    /// <inheritdoc />
    public partial class AddEmbeddingsToAllModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Point>(
                name: "Embedding",
                table: "Reviews",
                type: "geography",
                nullable: true);

            migrationBuilder.AddColumn<Point>(
                name: "Embedding",
                table: "Orders",
                type: "geography",
                nullable: true);

            migrationBuilder.AddColumn<Point>(
                name: "Embedding",
                table: "Categories",
                type: "geography",
                nullable: true);

            migrationBuilder.AddColumn<Point>(
                name: "Embedding",
                table: "Brands",
                type: "geography",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "Brands");
        }
    }
}
