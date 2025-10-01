using CsvHelper;
using CsvHelper.Configuration;
using ReorganizerGstvTech.Models;
using System.Globalization;

namespace ReorganizerGstvTech.Services;

public class CsvProcessorService
{
    private readonly CategoriaService _categoriaService;

    public CsvProcessorService(CategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    public async Task<List<Transacao>> ProcessarCsvAsync(Stream csvStream)
    {
        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            Delimiter = ";" // Usar ponto e vírgula como delimitador
        });

        var transacoes = new List<Transacao>();
        var registros = csv.GetRecords<dynamic>().ToList();

        foreach (var registro in registros)
        {
            try
            {
                // Extrair apenas os campos necessários: Descrição e Valor (em R$)
                var descricao = registro.Descrição?.ToString() ?? string.Empty;
                
                // Tentar diferentes nomes de campo para o valor usando reflexão
                var valorStr = "0";
                var registroDict = (IDictionary<string, object>)registro;
                
                // Procurar por campos que contenham "Valor" no nome
                foreach (var kvp in registroDict)
                {
                    if (kvp.Key.ToLower().Contains("valor") && kvp.Key.Contains("R"))
                    {
                        valorStr = kvp.Value?.ToString() ?? "0";
                        break;
                    }
                }
                
                // Se não encontrou, tentar o último campo (que geralmente é o valor em R$)
                if (valorStr == "0")
                {
                    var campos = registroDict.Keys.ToList();
                    if (campos.Count > 0)
                    {
                        var ultimoCampo = campos.Last();
                        valorStr = registroDict[ultimoCampo]?.ToString() ?? "0";
                    }
                }
                
                // Remover caracteres não numéricos exceto vírgula e ponto
                var valorLimpo = valorStr.Replace("R$", "").Replace(" ", "").Trim();
                
                // Converter valor considerando formato brasileiro (vírgula como separador decimal)
                var valor = decimal.TryParse(valorLimpo.Replace(",", "."), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal valorDecimal) 
                    ? valorDecimal 
                    : 0;

                if (!string.IsNullOrWhiteSpace(descricao) && valor > 0)
                {
                    var transacao = new Transacao
                    {
                        DataCompra = DateTime.Now, // Usar data atual já que não estamos processando a data
                        Descricao = descricao,
                        Parcela = 1, // Valor padrão já que não estamos processando parcela
                        Valor = valor
                    };

                    // Categorizar automaticamente
                    transacao.Categoria = await _categoriaService.CategorizarAsync(transacao.Descricao);
                    transacoes.Add(transacao);
                }
            }
            catch (Exception ex)
            {
                // Log do erro mas continua processando outros registros
                Console.WriteLine($"Erro ao processar registro: {ex.Message}");
                continue;
            }
        }

        return transacoes;
    }

    public List<CategoriaResumo> AgruparPorCategoria(List<Transacao> transacoes)
    {
        return transacoes
            .GroupBy(t => t.Categoria)
            .Select(g => new CategoriaResumo
            {
                Categoria = g.Key,
                Valor = g.Sum(t => t.Valor)
            })
            .OrderBy(c => c.Categoria)
            .ToList();
    }

    public async Task<byte[]> GerarCsvResumidoAsync(List<CategoriaResumo> resumos)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        });

        await csv.WriteRecordsAsync(resumos);
        await writer.FlushAsync();
        
        return memoryStream.ToArray();
    }
}
