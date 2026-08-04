using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddDamageFeeAndPaymentStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_paid",
                table: "damage_reports");

            migrationBuilder.AddColumn<decimal>(
                name: "damage_fee",
                table: "rentals",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "payment_status",
                table: "damage_reports",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Unpaid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "damage_fee",
                table: "rentals");

            migrationBuilder.DropColumn(
                name: "payment_status",
                table: "damage_reports");

            migrationBuilder.AddColumn<bool>(
                name: "is_paid",
                table: "damage_reports",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
