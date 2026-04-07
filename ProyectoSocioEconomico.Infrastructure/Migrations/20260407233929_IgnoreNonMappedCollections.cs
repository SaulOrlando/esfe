using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoSocioEconomico.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IgnoreNonMappedCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProgramaId",
                table: "Donaciones",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Donaciones_ProgramaId",
                table: "Donaciones",
                column: "ProgramaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Donaciones_Programas_ProgramaId",
                table: "Donaciones",
                column: "ProgramaId",
                principalTable: "Programas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Donaciones_Programas_ProgramaId",
                table: "Donaciones");

            migrationBuilder.DropIndex(
                name: "IX_Donaciones_ProgramaId",
                table: "Donaciones");

            migrationBuilder.DropColumn(
                name: "ProgramaId",
                table: "Donaciones");
        }
    }
}
