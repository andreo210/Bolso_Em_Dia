using System.Net;
using BolsoEmDia.Application.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BolsoEmDia.Tests.Api
{
    public class ProblemDetailsFactoriesTests
    {
        private static HttpContext Contexto(string path = "/api/v1/contas/7")
        {
            var contexto = new DefaultHttpContext();
            contexto.Request.Path = path;
            return contexto;
        }

        [Fact]
        public void ProblemFactory_recusa_status_de_sucesso()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ProblemFactory.Create(HttpStatusCode.OK, "ok"));
        }

        [Fact]
        public void ProblemFactory_com_status_de_erro_preenche_status_e_detalhe()
        {
            var problem = ProblemFactory.Create(HttpStatusCode.BadRequest, "Conta não encontrada", title: "Erro");

            Assert.Equal((int)HttpStatusCode.BadRequest, problem.Status);
            Assert.Equal("Erro", problem.Title);
            Assert.Equal("Conta não encontrada", problem.Detail);
        }

        [Fact]
        public void ExceptionProblemFactory_conflito_de_concorrencia_vira_409_com_mensagem_amigavel()
        {
            var problem = ExceptionProblemFactory.Create(
                Contexto(),
                new DbUpdateConcurrencyException("Row(s) affected by UPDATE do not match expected"));

            Assert.Equal((int)HttpStatusCode.Conflict, problem.Status);
            Assert.DoesNotContain("Row(s)", problem.Detail);
            Assert.DoesNotContain("UPDATE", problem.Detail);
        }

        [Fact]
        public void ExceptionProblemFactory_excecao_desconhecida_vira_500_com_a_propria_mensagem()
        {
            var problem = ExceptionProblemFactory.Create(Contexto(), new InvalidOperationException("Falha inesperada"));

            Assert.Equal((int)HttpStatusCode.InternalServerError, problem.Status);
            Assert.Equal("Falha inesperada", problem.Detail);
        }

        [Fact]
        public void ExceptionProblemFactory_problem_exception_preserva_o_proprio_status()
        {
            var original = ProblemFactory.Create(HttpStatusCode.NotFound, "Conta não encontrada");

            var problem = ExceptionProblemFactory.Create(Contexto(), new ProblemException(original));

            Assert.Equal((int)HttpStatusCode.NotFound, problem.Status);
            Assert.Equal("Conta não encontrada", problem.Detail);
        }

        [Fact]
        public void ExceptionProblemFactory_sempre_preenche_instance_e_traceid()
        {
            var contexto = Contexto("/api/v1/contas/7");

            var problem = ExceptionProblemFactory.Create(contexto, new InvalidOperationException("erro"));

            Assert.Equal("/api/v1/contas/7", problem.Instance);
            Assert.Equal(contexto.TraceIdentifier, problem.Extensions["traceId"]);
        }
    }
}
