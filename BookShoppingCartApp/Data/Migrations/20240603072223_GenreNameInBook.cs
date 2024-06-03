using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookShoppingCartApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class GenreNameInBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GenreName",
                table: "Book",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartDetail_ShoppingCartId",
                table: "CartDetail",
                column: "ShoppingCartId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartDetail_ShoppingCart_ShoppingCartId",
                table: "CartDetail",
                column: "ShoppingCartId",
                principalTable: "ShoppingCart",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartDetail_ShoppingCart_ShoppingCartId",
                table: "CartDetail");

            migrationBuilder.DropIndex(
                name: "IX_CartDetail_ShoppingCartId",
                table: "CartDetail");

            migrationBuilder.DropColumn(
                name: "GenreName",
                table: "Book");
        }
    }
}
