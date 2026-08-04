using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cars",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    year = table.Column<int>(type: "int", nullable: false),
                    current_mileage = table.Column<int>(type: "int", nullable: false),
                    maintenance_threshold_km = table.Column<int>(type: "int", nullable: false),
                    daily_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    extra_km_fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cars", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    license_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rentals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    car_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    planned_end_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    actual_return_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    start_mileage = table.Column<int>(type: "int", nullable: false),
                    end_mileage = table.Column<int>(type: "int", nullable: true),
                    base_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    late_fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    extra_km_charge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rentals", x => x.id);
                    table.ForeignKey(
                        name: "FK_rentals_cars_car_id",
                        column: x => x.car_id,
                        principalTable: "cars",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_rentals_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "damage_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    rental_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    damage_cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    is_paid = table.Column<bool>(type: "bit", nullable: false),
                    reported_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    paid_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_damage_reports", x => x.id);
                    table.ForeignKey(
                        name: "FK_damage_reports_rentals_rental_id",
                        column: x => x.rental_id,
                        principalTable: "rentals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_damage_reports_rental_id",
                table: "damage_reports",
                column: "rental_id");

            migrationBuilder.CreateIndex(
                name: "IX_rentals_car_id",
                table: "rentals",
                column: "car_id");

            migrationBuilder.CreateIndex(
                name: "IX_rentals_user_id",
                table: "rentals",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "damage_reports");

            migrationBuilder.DropTable(
                name: "rentals");

            migrationBuilder.DropTable(
                name: "cars");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
