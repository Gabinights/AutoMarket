using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMarket.Migrations
{
    /// <inheritdoc />
    public partial class InitialVeiculoSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Denuncias_Carros_TargetCarroId",
                table: "Denuncias");

            migrationBuilder.DropForeignKey(
                name: "FK_Mensagens_Carros_CarroId",
                table: "Mensagens");

            migrationBuilder.DropForeignKey(
                name: "FK_Transacoes_Carros_CarroId",
                table: "Transacoes");

            migrationBuilder.DropTable(
                name: "CarroImagens");

            migrationBuilder.DropTable(
                name: "Carros");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_NIF",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "CarroId",
                table: "Transacoes",
                newName: "VeiculoId");

            migrationBuilder.RenameIndex(
                name: "IX_Transacoes_CarroId",
                table: "Transacoes",
                newName: "IX_Transacoes_VeiculoId");

            migrationBuilder.RenameColumn(
                name: "CarroId",
                table: "Mensagens",
                newName: "VeiculoId");

            migrationBuilder.RenameIndex(
                name: "IX_Mensagens_CarroId",
                table: "Mensagens",
                newName: "IX_Mensagens_VeiculoId");

            migrationBuilder.RenameColumn(
                name: "TargetCarroId",
                table: "Denuncias",
                newName: "TargetVeiculoId");

            migrationBuilder.RenameIndex(
                name: "IX_Denuncias_TargetCarroId",
                table: "Denuncias",
                newName: "IX_Denuncias_TargetVeiculoId");

            migrationBuilder.CreateTable(
                name: "Veiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Marca = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Modelo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    Ano = table.Column<int>(type: "int", nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Km = table.Column<int>(type: "int", nullable: false),
                    Combustivel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Caixa = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Localizacao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Condicao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Descricao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VendedorId = table.Column<int>(type: "int", nullable: false),
                    CategoriaId1 = table.Column<int>(type: "int", nullable: true),
                    VendedorId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Veiculos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Veiculos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Veiculos_Categorias_CategoriaId1",
                        column: x => x.CategoriaId1,
                        principalTable: "Categorias",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Veiculos_Vendedores_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Vendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Veiculos_Vendedores_VendedorId1",
                        column: x => x.VendedorId1,
                        principalTable: "Vendedores",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VeiculoImagens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaminhoFicheiro = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsCapa = table.Column<bool>(type: "bit", nullable: false),
                    VeiculoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VeiculoImagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VeiculoImagens_Veiculos_VeiculoId",
                        column: x => x.VeiculoId,
                        principalTable: "Veiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VeiculoImagens_VeiculoId",
                table: "VeiculoImagens",
                column: "VeiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculo_Estado",
                table: "Veiculos",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculo_Marca",
                table: "Veiculos",
                column: "Marca");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculo_VendedorId",
                table: "Veiculos",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_CategoriaId",
                table: "Veiculos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_CategoriaId1",
                table: "Veiculos",
                column: "CategoriaId1");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_VendedorId1",
                table: "Veiculos",
                column: "VendedorId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Denuncias_Veiculos_TargetVeiculoId",
                table: "Denuncias",
                column: "TargetVeiculoId",
                principalTable: "Veiculos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Mensagens_Veiculos_VeiculoId",
                table: "Mensagens",
                column: "VeiculoId",
                principalTable: "Veiculos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transacoes_Veiculos_VeiculoId",
                table: "Transacoes",
                column: "VeiculoId",
                principalTable: "Veiculos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Denuncias_Veiculos_TargetVeiculoId",
                table: "Denuncias");

            migrationBuilder.DropForeignKey(
                name: "FK_Mensagens_Veiculos_VeiculoId",
                table: "Mensagens");

            migrationBuilder.DropForeignKey(
                name: "FK_Transacoes_Veiculos_VeiculoId",
                table: "Transacoes");

            migrationBuilder.DropTable(
                name: "VeiculoImagens");

            migrationBuilder.DropTable(
                name: "Veiculos");

            migrationBuilder.RenameColumn(
                name: "VeiculoId",
                table: "Transacoes",
                newName: "CarroId");

            migrationBuilder.RenameIndex(
                name: "IX_Transacoes_VeiculoId",
                table: "Transacoes",
                newName: "IX_Transacoes_CarroId");

            migrationBuilder.RenameColumn(
                name: "VeiculoId",
                table: "Mensagens",
                newName: "CarroId");

            migrationBuilder.RenameIndex(
                name: "IX_Mensagens_VeiculoId",
                table: "Mensagens",
                newName: "IX_Mensagens_CarroId");

            migrationBuilder.RenameColumn(
                name: "TargetVeiculoId",
                table: "Denuncias",
                newName: "TargetCarroId");

            migrationBuilder.RenameIndex(
                name: "IX_Denuncias_TargetVeiculoId",
                table: "Denuncias",
                newName: "IX_Denuncias_TargetCarroId");

            migrationBuilder.CreateTable(
                name: "Carros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    VendedorId = table.Column<int>(type: "int", nullable: false),
                    Ano = table.Column<int>(type: "int", nullable: false),
                    Caixa = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Combustivel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Km = table.Column<int>(type: "int", nullable: false),
                    Localizacao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Marca = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Modelo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Preco = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carros_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Carros_Vendedores_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Vendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarroImagens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarroId = table.Column<int>(type: "int", nullable: false),
                    CaminhoFicheiro = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsCapa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarroImagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarroImagens_Carros_CarroId",
                        column: x => x.CarroId,
                        principalTable: "Carros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_NIF",
                table: "AspNetUsers",
                column: "NIF",
                unique: true,
                filter: "[NIF] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CarroImagens_CarroId",
                table: "CarroImagens",
                column: "CarroId");

            migrationBuilder.CreateIndex(
                name: "IX_Carros_CategoriaId",
                table: "Carros",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Carros_VendedorId",
                table: "Carros",
                column: "VendedorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Denuncias_Carros_TargetCarroId",
                table: "Denuncias",
                column: "TargetCarroId",
                principalTable: "Carros",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Mensagens_Carros_CarroId",
                table: "Mensagens",
                column: "CarroId",
                principalTable: "Carros",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transacoes_Carros_CarroId",
                table: "Transacoes",
                column: "CarroId",
                principalTable: "Carros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
