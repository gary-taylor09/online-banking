using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineBankingSystem.Migrations.Banking
{
    /// <inheritdoc />
    public partial class AddDeletedColumnToAccountsAndTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                schema: "banking",
                table: "Transactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                schema: "banking",
                table: "Accounts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deleted",
                schema: "banking",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Deleted",
                schema: "banking",
                table: "Accounts");
        }
    }
}
