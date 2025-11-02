using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SparePartsWeb.Migrations
{
    public partial class RenameDescriptionToBrand : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Database already renamed manually, so no action needed here
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Optional rollback if you want to revert later
            // migrationBuilder.RenameColumn(
            //     name: "Brand",
            //     table: "Equipments",
            //     newName: "Description");
        }
    }
}
