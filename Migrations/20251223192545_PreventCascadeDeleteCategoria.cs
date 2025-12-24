using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMarket.Migrations
{
    /// <inheritdoc />
    public partial class PreventCascadeDeleteCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carros_Categorias_CategoriaId",
                table: "Carros");

            migrationBuilder.AddForeignKey(
                name: "FK_Carros_Categorias_CategoriaId",
                table: "Carros",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carros_Categorias_CategoriaId",
                table: "Carros");

            migrationBuilder.AddForeignKey(
                name: "FK_Carros_Categorias_CategoriaId",
                table: "Carros",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
