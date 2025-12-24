using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMarket.Migrations
{
    /// <inheritdoc />
    public partial class AjustesDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vendedores_UserId",
                table: "Vendedores");

            migrationBuilder.DropIndex(
                name: "IX_Compradores_UserId",
                table: "Compradores");

            migrationBuilder.RenameColumn(
                name: "IsPrincipale",
                table: "CarroImagens",
                newName: "IsCapa");

            migrationBuilder.AlterColumn<string>(
                name: "ApprovedByAdminId",
                table: "Vendedores",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AnalisadoPorAdminId",
                table: "Denuncias",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendedores_ApprovedByAdminId",
                table: "Vendedores",
                column: "ApprovedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendedores_UserId",
                table: "Vendedores",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Denuncias_AnalisadoPorAdminId",
                table: "Denuncias",
                column: "AnalisadoPorAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Compradores_UserId",
                table: "Compradores",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Denuncias_AspNetUsers_AnalisadoPorAdminId",
                table: "Denuncias",
                column: "AnalisadoPorAdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vendedores_AspNetUsers_ApprovedByAdminId",
                table: "Vendedores",
                column: "ApprovedByAdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Denuncias_AspNetUsers_AnalisadoPorAdminId",
                table: "Denuncias");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendedores_AspNetUsers_ApprovedByAdminId",
                table: "Vendedores");

            migrationBuilder.DropIndex(
                name: "IX_Vendedores_ApprovedByAdminId",
                table: "Vendedores");

            migrationBuilder.DropIndex(
                name: "IX_Vendedores_UserId",
                table: "Vendedores");

            migrationBuilder.DropIndex(
                name: "IX_Denuncias_AnalisadoPorAdminId",
                table: "Denuncias");

            migrationBuilder.DropIndex(
                name: "IX_Compradores_UserId",
                table: "Compradores");

            migrationBuilder.RenameColumn(
                name: "IsCapa",
                table: "CarroImagens",
                newName: "IsPrincipale");

            migrationBuilder.AlterColumn<string>(
                name: "ApprovedByAdminId",
                table: "Vendedores",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AnalisadoPorAdminId",
                table: "Denuncias",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendedores_UserId",
                table: "Vendedores",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Compradores_UserId",
                table: "Compradores",
                column: "UserId");
        }
    }
}
