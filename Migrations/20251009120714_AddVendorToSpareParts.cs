using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SparePartsWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorToSpareParts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VendorId",
                table: "SpareParts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpareParts_VendorId",
                table: "SpareParts",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_SpareParts_Vendors_VendorId",
                table: "SpareParts",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpareParts_Vendors_VendorId",
                table: "SpareParts");

            migrationBuilder.DropIndex(
                name: "IX_SpareParts_VendorId",
                table: "SpareParts");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "SpareParts");
        }
    }
}
