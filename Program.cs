using ReorganizerGstvTech.Services;
using ReorganizerGstvTech.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<CsvProcessorService>();
builder.Services.AddScoped<CategoriaService>();

var app = builder.Build();

// Configurar porta personalizada
var port = builder.Configuration["Port"] ?? "5003";
var url = $"http://localhost:{port}";

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

// Endpoint para upload e processamento de CSV
app.MapPost("/upload", async (HttpContext context, CsvProcessorService csvProcessor, CategoriaService categoriaService) =>
{
    try
    {
        var form = await context.Request.ReadFormAsync();
        var file = form.Files.FirstOrDefault();

        if (file == null || file.Length == 0)
            return Results.BadRequest(new { error = "Arquivo não fornecido" });

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest(new { error = "Arquivo deve ser um CSV" });

        using var stream = file.OpenReadStream();
        var transacoes = await csvProcessor.ProcessarCsvAsync(stream);
        var resumos = csvProcessor.AgruparPorCategoria(transacoes);
        var csvBytes = await csvProcessor.GerarCsvResumidoAsync(resumos);

        var naoCategorizadas = categoriaService.ObterCategoriasNaoReconhecidas(transacoes);

        return Results.Ok(new
        {
            csvProcessado = Convert.ToBase64String(csvBytes),
            naoCategorizadas = naoCategorizadas,
            resumos = resumos
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: $"Erro ao processar arquivo: {ex.Message}", statusCode: 500);
    }
});

// Endpoint para categorizar manualmente
app.MapPost("/categorize", async (HttpContext context, CategoriaService categoriaService) =>
{
    try
    {
        var request = await context.Request.ReadFromJsonAsync<CategorizacaoRequest>();
        
        if (request == null || string.IsNullOrWhiteSpace(request.Descricao) || string.IsNullOrWhiteSpace(request.Categoria))
            return Results.BadRequest(new { error = "Descrição e categoria são obrigatórias" });

        await categoriaService.AdicionarCategorizacaoAsync(request.Descricao, request.Categoria);
        
        return Results.Ok(new { message = "Categorização salva com sucesso" });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: $"Erro ao salvar categorização: {ex.Message}", statusCode: 500);
    }
});

app.Run(url);
