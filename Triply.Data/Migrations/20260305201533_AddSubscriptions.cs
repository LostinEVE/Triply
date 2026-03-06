using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Triply.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    SubscriptionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Tier = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    TrialStartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TrialEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SubscriptionStartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SubscriptionEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PurchaseToken = table.Column<string>(type: "TEXT", nullable: true),
                    IsAutoRenew = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastCheckedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.SubscriptionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Subscriptions");
        }
    }
}
