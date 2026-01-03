using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMarket.Migrations
{
    /// <inheritdoc />
    public partial class AddVendedorIdToTransacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adicionar coluna VendedorId (nullable temporariamente)
            migrationBuilder.AddColumn<int>(
                name: "VendedorId",
                table: "Transacoes",
                type: "int",
                nullable: true);

            // Preencher VendedorId para transações existentes baseado no Veiculo
            migrationBuilder.Sql(@"
                UPDATE t
                SET t.VendedorId = v.VendedorId
                FROM Transacoes t
                INNER JOIN Veiculos v ON t.VeiculoId = v.Id
                WHERE t.VendedorId IS NULL
            ");

            // Tornar coluna NOT NULL
            migrationBuilder.AlterColumn<int>(
                name: "VendedorId",
                table: "Transacoes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transacoes_VendedorId",
                table: "Transacoes",
                column: "VendedorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transacoes_Vendedores_VendedorId",
                table: "Transacoes",
                column: "VendedorId",
                principalTable: "Vendedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transacoes_Vendedores_VendedorId",
                table: "Transacoes");

            migrationBuilder.DropIndex(
                name: "IX_Transacoes_VendedorId",
                table: "Transacoes");

            migrationBuilder.DropColumn(
                name: "VendedorId",
                table: "Transacoes");
        }
    }
}
