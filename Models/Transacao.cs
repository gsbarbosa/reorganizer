namespace ReorganizerGstvTech.Models;

public class Transacao
{
    public DateTime DataCompra { get; set; } = DateTime.Now;
    public string Descricao { get; set; } = string.Empty;
    public int Parcela { get; set; } = 1;
    public decimal Valor { get; set; }
    public string Categoria { get; set; } = "Não Categorizado";
}
