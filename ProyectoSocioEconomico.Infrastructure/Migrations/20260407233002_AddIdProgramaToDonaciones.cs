using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoSocioEconomico.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdProgramaToDonaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "IdCaso",
                table: "Donaciones",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "IdPrograma",
                table: "Donaciones",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Donaciones_IdPrograma",
                table: "Donaciones",
                column: "IdPrograma");

            migrationBuilder.AddForeignKey(
                name: "FK_Donaciones_Programas_IdPrograma",
                table: "Donaciones",
                column: "IdPrograma",
                principalTable: "Programas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Donaciones_Programas_IdPrograma",
                table: "Donaciones");

            migrationBuilder.DropIndex(
                name: "IX_Donaciones_IdPrograma",
                table: "Donaciones");

            migrationBuilder.DropColumn(
                name: "IdPrograma",
                table: "Donaciones");

            migrationBuilder.AlterColumn<int>(
                name: "IdCaso",
                table: "Donaciones",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
