# Reorganizer GSTV Tech

Sistema web em .NET 8 (Minimal API) para processamento e categorização de planilhas CSV.

## Funcionalidades

- 📁 Upload de arquivos CSV com colunas: Descrição e Valor (em R$)
- 🤖 Categorização automática baseada em descrições conhecidas
- 🧠 Aprendizado contínuo através de categorizações manuais
- 📊 Agrupamento e resumo por categoria
- 💾 Persistência em arquivo JSON local
- 🎨 Interface web moderna e responsiva

## Como Usar

1. **Executar o projeto:**
   ```bash
   dotnet run
   ```

2. **Configurar porta personalizada:**
   - Edite o arquivo `appsettings.json` e altere o valor de `"Port"`
   - Ou execute com variável de ambiente: `Port=8080 dotnet run`
   - Ou passe como argumento: `dotnet run --Port=8080`

3. **Acessar a aplicação:**
   - A aplicação rodará na porta configurada (padrão: 5000)
   - Exemplo: `http://localhost:5000` ou `http://localhost:8080`

4. **Processar CSV:**
   - Faça upload de um arquivo CSV
   - O sistema processará e categorizará automaticamente
   - Baixe o CSV processado com o resumo por categoria

5. **Categorização manual:**
   - Para descrições não reconhecidas, defina manualmente a categoria
   - O sistema aprenderá com suas categorizações para futuros processamentos

## Estrutura do Projeto

```
ReorganizerGstvTech/
├── Models/                 # Modelos de dados
│   ├── Transacao.cs
│   ├── CategoriaResumo.cs
│   └── CategorizacaoRequest.cs
├── Services/              # Serviços de negócio
│   ├── CsvProcessorService.cs
│   └── CategoriaService.cs
├── wwwroot/               # Arquivos estáticos
│   └── index.html
├── Program.cs             # Configuração da API
└── categorias.json        # Base de conhecimento (criado automaticamente)
```

## Tecnologias Utilizadas

- .NET 8 Minimal API
- CsvHelper para processamento de CSV
- HTML5, CSS3, JavaScript vanilla
- JSON para persistência de dados

## Exemplo de CSV de Entrada

```csv
Data de Compra;Nome no Cartão;Final do Cartão;Categoria;Descrição;Parcela;Valor (em US$);Cotação (em R$);Valor (em R$)
01/10/2024;GUSTAVO S BARBOSA;0880;Assistência médica e odontológica;MT-E ODONTO LTDA;11/12;0;0;191.66
01/11/2024;GUSTAVO S BARBOSA;0880;Marketing Direto;PG *BANDNEST PRODUCOES;10/12;0;0;196.99
29/11/2024;GUSTAVO S BARBOSA;0880;Departamento / Desconto;AMAZON BR;9/12;0;0;53.32
```

**Nota:** O sistema processará apenas os campos "Descrição" e "Valor (em R$)", ignorando os demais campos.

## Exemplo de CSV de Saída

```csv
Categoria,Valor
Netflix,45.90
Supermercado,150.00
Uber,61.30
```

## Persistência de Categorias

O arquivo `categorias.json` é criado automaticamente e contém o mapeamento entre descrições e categorias:

```json
{
  "Uber": [
    "Uber Viagem",
    "Uber Eats"
  ],
  "Supermercado": [
    "Supermercado Extra",
    "Mercado Central"
  ]
}
```
