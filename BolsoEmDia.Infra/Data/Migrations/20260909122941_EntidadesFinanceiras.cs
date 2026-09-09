using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BolsoEmDia.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class EntidadesFinanceiras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    id_categoria = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    id_categoria_pai = table.Column<int>(type: "integer", nullable: true),
                    ativa = table.Column<bool>(type: "boolean", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id_usuario_criacao = table.Column<string>(type: "text", nullable: true),
                    data_modificacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_usuario_modificacao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categorias", x => x.id_categoria);
                    table.ForeignKey(
                        name: "fk_categorias_asp_net_users_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_categorias_categorias_id_categoria_pai",
                        column: x => x.id_categoria_pai,
                        principalTable: "categorias",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "contas",
                columns: table => new
                {
                    id_conta = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    saldo_inicial = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ativa = table.Column<bool>(type: "boolean", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id_usuario_criacao = table.Column<string>(type: "text", nullable: true),
                    data_modificacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_usuario_modificacao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contas", x => x.id_conta);
                    table.ForeignKey(
                        name: "fk_contas_asp_net_users_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orcamentos",
                columns: table => new
                {
                    id_orcamento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<string>(type: "text", nullable: false),
                    id_categoria = table.Column<int>(type: "integer", nullable: false),
                    mes_referencia = table.Column<DateOnly>(type: "date", nullable: false),
                    valor_meta = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id_usuario_criacao = table.Column<string>(type: "text", nullable: true),
                    data_modificacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_usuario_modificacao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orcamentos", x => x.id_orcamento);
                    table.ForeignKey(
                        name: "fk_orcamentos_asp_net_users_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_orcamentos_categorias_id_categoria",
                        column: x => x.id_categoria,
                        principalTable: "categorias",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cartoes",
                columns: table => new
                {
                    id_cartao = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<string>(type: "text", nullable: false),
                    id_conta_pagamento = table.Column<int>(type: "integer", nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    limite_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    dia_fechamento = table.Column<int>(type: "integer", nullable: false),
                    dia_vencimento = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id_usuario_criacao = table.Column<string>(type: "text", nullable: true),
                    data_modificacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_usuario_modificacao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cartoes", x => x.id_cartao);
                    table.ForeignKey(
                        name: "fk_cartoes_asp_net_users_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_cartoes_contas_id_conta_pagamento",
                        column: x => x.id_conta_pagamento,
                        principalTable: "contas",
                        principalColumn: "id_conta",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "metas_economia",
                columns: table => new
                {
                    id_meta = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<string>(type: "text", nullable: false),
                    id_conta = table.Column<int>(type: "integer", nullable: true),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    valor_alvo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_alvo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    concluida = table.Column<bool>(type: "boolean", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id_usuario_criacao = table.Column<string>(type: "text", nullable: true),
                    data_modificacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_usuario_modificacao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_metas_economia", x => x.id_meta);
                    table.ForeignKey(
                        name: "fk_metas_economia_asp_net_users_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_metas_economia_contas_id_conta",
                        column: x => x.id_conta,
                        principalTable: "contas",
                        principalColumn: "id_conta",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "transferencias",
                columns: table => new
                {
                    id_transferencia = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<string>(type: "text", nullable: false),
                    id_conta_origem = table.Column<int>(type: "integer", nullable: false),
                    id_conta_destino = table.Column<int>(type: "integer", nullable: false),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    descricao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id_usuario_criacao = table.Column<string>(type: "text", nullable: true),
                    data_modificacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_usuario_modificacao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transferencias", x => x.id_transferencia);
                    table.ForeignKey(
                        name: "fk_transferencias_asp_net_users_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_transferencias_contas_id_conta_destino",
                        column: x => x.id_conta_destino,
                        principalTable: "contas",
                        principalColumn: "id_conta",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transferencias_contas_id_conta_origem",
                        column: x => x.id_conta_origem,
                        principalTable: "contas",
                        principalColumn: "id_conta",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "recorrencias",
                columns: table => new
                {
                    id_recorrencia = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<string>(type: "text", nullable: false),
                    id_conta = table.Column<int>(type: "integer", nullable: true),
                    id_cartao = table.Column<int>(type: "integer", nullable: true),
                    id_categoria = table.Column<int>(type: "integer", nullable: false),
                    tipo_transacao = table.Column<int>(type: "integer", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    frequencia = table.Column<int>(type: "integer", nullable: false),
                    dia_geracao = table.Column<int>(type: "integer", nullable: false),
                    data_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ativa = table.Column<bool>(type: "boolean", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id_usuario_criacao = table.Column<string>(type: "text", nullable: true),
                    data_modificacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_usuario_modificacao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recorrencias", x => x.id_recorrencia);
                    table.ForeignKey(
                        name: "fk_recorrencias_asp_net_users_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_recorrencias_cartoes_id_cartao",
                        column: x => x.id_cartao,
                        principalTable: "cartoes",
                        principalColumn: "id_cartao",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_recorrencias_categorias_id_categoria",
                        column: x => x.id_categoria,
                        principalTable: "categorias",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_recorrencias_contas_id_conta",
                        column: x => x.id_conta,
                        principalTable: "contas",
                        principalColumn: "id_conta",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compras",
                columns: table => new
                {
                    id_compra = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<string>(type: "text", nullable: false),
                    id_cartao = table.Column<int>(type: "integer", nullable: false),
                    id_categoria = table.Column<int>(type: "integer", nullable: false),
                    id_recorrencia = table.Column<int>(type: "integer", nullable: true),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    descricao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    numero_parcelas = table.Column<int>(type: "integer", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id_usuario_criacao = table.Column<string>(type: "text", nullable: true),
                    data_modificacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_usuario_modificacao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_compras", x => x.id_compra);
                    table.ForeignKey(
                        name: "fk_compras_asp_net_users_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_compras_cartoes_id_cartao",
                        column: x => x.id_cartao,
                        principalTable: "cartoes",
                        principalColumn: "id_cartao",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_compras_categorias_id_categoria",
                        column: x => x.id_categoria,
                        principalTable: "categorias",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_compras_recorrencias_id_recorrencia",
                        column: x => x.id_recorrencia,
                        principalTable: "recorrencias",
                        principalColumn: "id_recorrencia",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "transacoes",
                columns: table => new
                {
                    id_transacao = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<string>(type: "text", nullable: false),
                    id_conta = table.Column<int>(type: "integer", nullable: false),
                    id_categoria = table.Column<int>(type: "integer", nullable: true),
                    id_transferencia = table.Column<int>(type: "integer", nullable: true),
                    id_recorrencia = table.Column<int>(type: "integer", nullable: true),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    descricao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id_usuario_criacao = table.Column<string>(type: "text", nullable: true),
                    data_modificacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_usuario_modificacao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transacoes", x => x.id_transacao);
                    table.ForeignKey(
                        name: "fk_transacoes_asp_net_users_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_transacoes_categorias_id_categoria",
                        column: x => x.id_categoria,
                        principalTable: "categorias",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_transacoes_contas_id_conta",
                        column: x => x.id_conta,
                        principalTable: "contas",
                        principalColumn: "id_conta",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transacoes_recorrencias_id_recorrencia",
                        column: x => x.id_recorrencia,
                        principalTable: "recorrencias",
                        principalColumn: "id_recorrencia",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_transacoes_transferencias_id_transferencia",
                        column: x => x.id_transferencia,
                        principalTable: "transferencias",
                        principalColumn: "id_transferencia",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aportes_meta",
                columns: table => new
                {
                    id_aporte = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<string>(type: "text", nullable: false),
                    id_meta = table.Column<int>(type: "integer", nullable: false),
                    id_transacao = table.Column<int>(type: "integer", nullable: true),
                    data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id_usuario_criacao = table.Column<string>(type: "text", nullable: true),
                    data_modificacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_usuario_modificacao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_aportes_meta", x => x.id_aporte);
                    table.ForeignKey(
                        name: "fk_aportes_meta_asp_net_users_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_aportes_meta_metas_economia_id_meta",
                        column: x => x.id_meta,
                        principalTable: "metas_economia",
                        principalColumn: "id_meta",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_aportes_meta_transacoes_id_transacao",
                        column: x => x.id_transacao,
                        principalTable: "transacoes",
                        principalColumn: "id_transacao",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "faturas",
                columns: table => new
                {
                    id_fatura = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<string>(type: "text", nullable: false),
                    id_cartao = table.Column<int>(type: "integer", nullable: false),
                    id_transacao_pagamento = table.Column<int>(type: "integer", nullable: true),
                    mes_referencia = table.Column<DateOnly>(type: "date", nullable: false),
                    data_fechamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_vencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id_usuario_criacao = table.Column<string>(type: "text", nullable: true),
                    data_modificacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_usuario_modificacao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_faturas", x => x.id_fatura);
                    table.ForeignKey(
                        name: "fk_faturas_asp_net_users_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_faturas_cartoes_id_cartao",
                        column: x => x.id_cartao,
                        principalTable: "cartoes",
                        principalColumn: "id_cartao",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_faturas_transacoes_id_transacao_pagamento",
                        column: x => x.id_transacao_pagamento,
                        principalTable: "transacoes",
                        principalColumn: "id_transacao",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "parcelas",
                columns: table => new
                {
                    id_parcela = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_usuario = table.Column<string>(type: "text", nullable: false),
                    id_compra = table.Column<int>(type: "integer", nullable: false),
                    id_fatura = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id_usuario_criacao = table.Column<string>(type: "text", nullable: true),
                    data_modificacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    id_usuario_modificacao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_parcelas", x => x.id_parcela);
                    table.ForeignKey(
                        name: "fk_parcelas_asp_net_users_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_parcelas_compras_id_compra",
                        column: x => x.id_compra,
                        principalTable: "compras",
                        principalColumn: "id_compra",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_parcelas_faturas_id_fatura",
                        column: x => x.id_fatura,
                        principalTable: "faturas",
                        principalColumn: "id_fatura",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_aportes_meta_id_meta",
                table: "aportes_meta",
                column: "id_meta");

            migrationBuilder.CreateIndex(
                name: "ix_aportes_meta_id_transacao",
                table: "aportes_meta",
                column: "id_transacao");

            migrationBuilder.CreateIndex(
                name: "ix_aportes_meta_id_usuario",
                table: "aportes_meta",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_cartoes_id_conta_pagamento",
                table: "cartoes",
                column: "id_conta_pagamento");

            migrationBuilder.CreateIndex(
                name: "ix_cartoes_id_usuario",
                table: "cartoes",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_categorias_id_categoria_pai",
                table: "categorias",
                column: "id_categoria_pai");

            migrationBuilder.CreateIndex(
                name: "ix_categorias_id_usuario",
                table: "categorias",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_compras_id_cartao",
                table: "compras",
                column: "id_cartao");

            migrationBuilder.CreateIndex(
                name: "ix_compras_id_categoria",
                table: "compras",
                column: "id_categoria");

            migrationBuilder.CreateIndex(
                name: "ix_compras_id_recorrencia",
                table: "compras",
                column: "id_recorrencia");

            migrationBuilder.CreateIndex(
                name: "ix_compras_id_usuario",
                table: "compras",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_contas_id_usuario",
                table: "contas",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_faturas_id_cartao_mes_referencia",
                table: "faturas",
                columns: new[] { "id_cartao", "mes_referencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_faturas_id_transacao_pagamento",
                table: "faturas",
                column: "id_transacao_pagamento");

            migrationBuilder.CreateIndex(
                name: "ix_faturas_id_usuario",
                table: "faturas",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_metas_economia_id_conta",
                table: "metas_economia",
                column: "id_conta");

            migrationBuilder.CreateIndex(
                name: "ix_metas_economia_id_usuario",
                table: "metas_economia",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_orcamentos_id_categoria_mes_referencia",
                table: "orcamentos",
                columns: new[] { "id_categoria", "mes_referencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_orcamentos_id_usuario",
                table: "orcamentos",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_parcelas_id_compra",
                table: "parcelas",
                column: "id_compra");

            migrationBuilder.CreateIndex(
                name: "ix_parcelas_id_fatura",
                table: "parcelas",
                column: "id_fatura");

            migrationBuilder.CreateIndex(
                name: "ix_parcelas_id_usuario",
                table: "parcelas",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_recorrencias_id_cartao",
                table: "recorrencias",
                column: "id_cartao");

            migrationBuilder.CreateIndex(
                name: "ix_recorrencias_id_categoria",
                table: "recorrencias",
                column: "id_categoria");

            migrationBuilder.CreateIndex(
                name: "ix_recorrencias_id_conta",
                table: "recorrencias",
                column: "id_conta");

            migrationBuilder.CreateIndex(
                name: "ix_recorrencias_id_usuario",
                table: "recorrencias",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_transacoes_id_categoria",
                table: "transacoes",
                column: "id_categoria");

            migrationBuilder.CreateIndex(
                name: "ix_transacoes_id_conta",
                table: "transacoes",
                column: "id_conta");

            migrationBuilder.CreateIndex(
                name: "ix_transacoes_id_recorrencia",
                table: "transacoes",
                column: "id_recorrencia");

            migrationBuilder.CreateIndex(
                name: "ix_transacoes_id_transferencia",
                table: "transacoes",
                column: "id_transferencia");

            migrationBuilder.CreateIndex(
                name: "ix_transacoes_id_usuario",
                table: "transacoes",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_transferencias_id_conta_destino",
                table: "transferencias",
                column: "id_conta_destino");

            migrationBuilder.CreateIndex(
                name: "ix_transferencias_id_conta_origem",
                table: "transferencias",
                column: "id_conta_origem");

            migrationBuilder.CreateIndex(
                name: "ix_transferencias_id_usuario",
                table: "transferencias",
                column: "id_usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aportes_meta");

            migrationBuilder.DropTable(
                name: "orcamentos");

            migrationBuilder.DropTable(
                name: "parcelas");

            migrationBuilder.DropTable(
                name: "metas_economia");

            migrationBuilder.DropTable(
                name: "compras");

            migrationBuilder.DropTable(
                name: "faturas");

            migrationBuilder.DropTable(
                name: "transacoes");

            migrationBuilder.DropTable(
                name: "recorrencias");

            migrationBuilder.DropTable(
                name: "transferencias");

            migrationBuilder.DropTable(
                name: "cartoes");

            migrationBuilder.DropTable(
                name: "categorias");

            migrationBuilder.DropTable(
                name: "contas");
        }
    }
}
