using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineBankingSystem.Migrations.Banking
{
    /// <inheritdoc />
    public partial class AddAccountType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountTypeID",
                schema: "banking",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AccountTypes",
                schema: "banking",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountTypes", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccountTypeID",
                schema: "banking",
                table: "Accounts",
                column: "AccountTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_AccountTypes_AccountTypeID",
                schema: "banking",
                table: "Accounts",
                column: "AccountTypeID",
                principalSchema: "banking",
                principalTable: "AccountTypes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_AccountTypes_AccountTypeID",
                schema: "banking",
                table: "Accounts");

            migrationBuilder.DropTable(
                name: "AccountTypes",
                schema: "banking");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_AccountTypeID",
                schema: "banking",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "AccountTypeID",
                schema: "banking",
                table: "Accounts");
        }
    }
}
