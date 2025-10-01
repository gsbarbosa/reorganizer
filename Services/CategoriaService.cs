using ReorganizerGstvTech.Models;
using System.Text.Json;

namespace ReorganizerGstvTech.Services;

public class CategoriaService
{
    private readonly string _categoriasFilePath = "categorias.json";
    private Dictionary<string, List<string>> _categorias = new();

    public CategoriaService()
    {
        CarregarCategorias();
    }

    private void CarregarCategorias()
    {
        if (File.Exists(_categoriasFilePath))
        {
            try
            {
                var json = File.ReadAllText(_categoriasFilePath);
                _categorias = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json) ?? new();
            }
            catch
            {
                _categorias = new();
            }
        }
    }

    private async Task SalvarCategoriasAsync()
    {
        var json = JsonSerializer.Serialize(_categorias, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_categoriasFilePath, json);
    }

    public Task<string> CategorizarAsync(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            return Task.FromResult("Não Categorizado");

        var descricaoLower = descricao.ToLowerInvariant();

        foreach (var categoria in _categorias)
        {
            if (categoria.Value.Any(desc => descricaoLower.Contains(desc.ToLowerInvariant())))
            {
                return Task.FromResult(categoria.Key);
            }
        }

        return Task.FromResult("Não Categorizado");
    }

    public async Task AdicionarCategorizacaoAsync(string descricao, string categoria)
    {
        if (string.IsNullOrWhiteSpace(descricao) || string.IsNullOrWhiteSpace(categoria))
            return;

        if (!_categorias.ContainsKey(categoria))
        {
            _categorias[categoria] = new List<string>();
        }

        if (!_categorias[categoria].Contains(descricao))
        {
            _categorias[categoria].Add(descricao);
            await SalvarCategoriasAsync();
        }
    }

    public List<string> ObterCategoriasNaoReconhecidas(List<Transacao> transacoes)
    {
        return transacoes
            .Where(t => t.Categoria == "Não Categorizado")
            .Select(t => t.Descricao)
            .Distinct()
            .ToList();
    }
}
