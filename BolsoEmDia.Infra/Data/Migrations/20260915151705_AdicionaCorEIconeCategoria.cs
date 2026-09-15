using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BolsoEmDia.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaCorEIconeCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cor",
                table: "categorias",
                type: "character varying(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "#6c757d");

            migrationBuilder.AddColumn<string>(
                name: "icone",
                table: "categorias",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "bi-tag");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cor",
                table: "categorias");

            migrationBuilder.DropColumn(
                name: "icone",
                table: "categorias");
        }
    }
}
