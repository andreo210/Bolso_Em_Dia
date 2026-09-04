namespace BolsoEmDia.Domain
{
    /// <summary>
    /// Marca entidades de negócio que pertencem a um usuário. O AppDbContext usa essa marca
    /// para aplicar um global query filter (IdUsuario == usuário logado) em todas elas de uma
    /// vez, em vez de repetir HasQueryFilter entidade por entidade — um filtro esquecido aqui
    /// vazaria dado financeiro de um usuário para outro.
    /// </summary>
    public interface IPertenceAoUsuario
    {
        string IdUsuario { get; }
    }
}
