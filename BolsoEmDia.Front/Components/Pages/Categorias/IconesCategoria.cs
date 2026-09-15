namespace BolsoEmDia.Front.Components.Pages.Categorias
{
    // Conjunto curado de Bootstrap Icons (já vendorizado em wwwroot/lib/bootstrap-icons)
    // cobrindo as categorias mais comuns de um orçamento pessoal.
    public static class IconesCategoria
    {
        public static readonly IReadOnlyList<(string Classe, string Rotulo)> Opcoes = new List<(string, string)>
        {
            ("bi-tag", "Geral"),
            ("bi-cart", "Compras"),
            ("bi-house", "Moradia"),
            ("bi-car-front", "Transporte"),
            ("bi-cup-hot", "Alimentação"),
            ("bi-heart-pulse", "Saúde"),
            ("bi-mortarboard", "Educação"),
            ("bi-airplane", "Viagem"),
            ("bi-controller", "Lazer"),
            ("bi-gift", "Presentes"),
            ("bi-piggy-bank", "Poupança"),
            ("bi-cash-coin", "Salário"),
            ("bi-briefcase", "Trabalho"),
            ("bi-phone", "Telefone/Internet"),
            ("bi-lightning-charge", "Contas/Energia"),
            ("bi-paw", "Pets"),
            ("bi-tools", "Manutenção"),
            ("bi-bag-heart", "Vestuário"),
            ("bi-film", "Assinaturas"),
            ("bi-three-dots", "Outros"),
        };
    }
}
