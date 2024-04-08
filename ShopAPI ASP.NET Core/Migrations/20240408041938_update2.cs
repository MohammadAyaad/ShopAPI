using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopAPI.Migrations
{
    /// <inheritdoc />
    public partial class update2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                table: "UserAccounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "UserAccounts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
