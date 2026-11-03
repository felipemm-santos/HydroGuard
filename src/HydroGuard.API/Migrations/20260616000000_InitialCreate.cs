using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HydroGuard.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Medidores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Serial = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Localizacao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medidores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Medidores_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alertas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedidorId = table.Column<int>(type: "int", nullable: false),
                    VazamentoDetectado = table.Column<bool>(type: "bit", nullable: false),
                    Severidade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mensagem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataAlerta = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alertas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alertas_Medidores_MedidorId",
                        column: x => x.MedidorId,
                        principalTable: "Medidores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Consumos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedidorId = table.Column<int>(type: "int", nullable: false),
                    LitrosConsumidos = table.Column<double>(type: "float", nullable: false),
                    DataRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consumos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consumos_Medidores_MedidorId",
                        column: x => x.MedidorId,
                        principalTable: "Medidores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alertas_MedidorId",
                table: "Alertas",
                column: "MedidorId");

            migrationBuilder.CreateIndex(
                name: "IX_Consumos_MedidorId",
                table: "Consumos",
                column: "MedidorId");

            migrationBuilder.CreateIndex(
                name: "IX_Medidores_Serial",
                table: "Medidores",
                column: "Serial",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medidores_UserId",
                table: "Medidores",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Alertas");
            migrationBuilder.DropTable(name: "Consumos");
            migrationBuilder.DropTable(name: "Medidores");
            migrationBuilder.DropTable(name: "Users");
        }
    }
}
