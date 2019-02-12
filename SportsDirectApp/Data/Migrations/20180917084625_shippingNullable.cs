using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SportsDirectApp.Data.Migrations
{
    public partial class shippingNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Shipping",
                table: "Order",
                nullable: true,
                oldClrType: typeof(decimal));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Shipping",
                table: "Order",
                nullable: false,
                oldClrType: typeof(decimal),
                oldNullable: true);
        }
    }
}
