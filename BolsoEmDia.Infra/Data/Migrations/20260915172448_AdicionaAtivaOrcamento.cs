using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BolsoEmDia.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaAtivaOrcamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ativa",
                table: "orcamentos",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ativa",
                table: "orcamentos");
        }
    }
}
