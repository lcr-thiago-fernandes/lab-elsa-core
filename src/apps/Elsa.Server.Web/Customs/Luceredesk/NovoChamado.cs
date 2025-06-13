using Elsa.Workflows.Attributes;
using Elsa.Workflows;
using System.Text;
using System.Text.Json;
using Elsa.Workflows.Models;
using Elsa.Extensions;
using Elsa.Expressions.Models;

namespace Elsa.Server.Web.Customs.Luceredesk;

[Activity("Lucere", "Novo chamado", DisplayName = "Novo Chamado")]
public class NovoChamado : Activity
{

    [Input(
        Description = "Título do chamado",
        Category = "Ticket"
    )]
    public Input<string> Titulo { get; set; } = null!;

    [Input(
        Description = "Descrição do chamado",
        Category = "Ticket"
    )]
    public Input<string> Descricao { get; set; } = null!;

    [Input(
    Description = "Tipo",
    Options = new[] {
        "Dúvida ou Outros",
        "Melhoria",
        "Problema"
    },
    Category = "Ticket"
    )]
    public Input<IDictionary<string, string>> Tipo { get; set; } = default!;

    [Input(
        Description = "Catálogo",
        Options = new[] {
            "Criação de novo módulo no sistema",
            "Criação de novos campos em formulários",
            "Lentidão e intermitência no sistema",
            "Problema no envio de e-mails",
            "Sistema fora do ar",
            "Solicitação de suporte para utilização do sistema",
            "Treinamento"
        },
        Category = "Ticket"
    )]
    public Input<IDictionary<string, string>> Catalogo { get; set; } = default!;

    public Output<string> Message { get; set; } = default!;

    private Dictionary<string, string> _tipos { get; set; } = new Dictionary<string, string>
    {
        { "Dúvida ou Outros", "CA0D1440-CCB3-4ACB-9EEE-1D4C4E97E44F" },
        { "Melhoria", "BC34D207-80F6-407D-A8F2-5563B98D6ED3" },
        { "Problema", "D6996F2F-A3AE-49DF-90F2-D80A591E5A9F" }
    };

    private Dictionary<string, string> _catalogos { get; set; } = new Dictionary<string, string>
    {
        { "Criação de novo módulo no sistema", "C2498759-A7B8-4657-9FBD-11BB4B71B8A0" },
        { "Criação de novos campos em formulários", "8042CB55-50B7-443D-805D-D4CB83B200C2" },
        { "Lentidão e intermitência no sistema", "9ABC56F4-B600-4C14-A096-202058FB4AAA" },
        { "Problema no envio de e-mails", "8A1E335A-D1DD-4131-90B5-D3C83C0D68AE" },
        { "Sistema fora do ar", "A31D244D-E3B0-422F-8A89-E14CFBEEEC49" },
        { "Solicitação de suporte para utilização do sistema", "9FCE1186-A1AB-4643-B932-8BF6F401E49B" },
        { "Treinamento", "E0B0D498-631B-483F-BA64-9FEE8F7BF301" }
    };

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {

        var config = context.WorkflowExecutionContext.ServiceProvider.GetRequiredService<IConfiguration>();

        var webhookUrl = config.GetSection("Webhooks:Sinks")
            .GetChildren()
            .FirstOrDefault(s => s["Id"] == "1")?["Url"];

        //var parametros = context.WorkflowInput["Parametros"];

        string? tokenUsuario = context.GetVariable<string>("TokenUsuario");

        if (string.IsNullOrEmpty(tokenUsuario))
        {
            tokenUsuario = context.WorkflowInput["TokenUsuario"]?.ToString();
            context.SetVariable("TokenUsuario", tokenUsuario);
        }

        string? sistemaId = context.GetVariable<string>("SistemaId");
        if (string.IsNullOrEmpty(sistemaId))
        {
            sistemaId = context.WorkflowInput["SistemaId"]?.ToString();
            context.SetVariable("SistemaId", tokenUsuario);
        }        

        var inputTitulo = (string)Titulo.Expression.Value;
        var inputDescricao = (string)Descricao.Expression.Value;
        var inputTipo = (from tipo in _tipos where tipo.Key == (string)Tipo.Expression.Value select tipo.Value).First();
        var inputCatalogo = (from ct in _catalogos where ct.Key == (string)Catalogo.Expression.Value select ct.Value).First();

        var payload = new
        {
            Event = "NovoChamado",
            Chamado = new
            {
                TipoId = inputTipo,
                Numero = 0,//
                Backlog = false,//
                Titulo = inputTitulo,
                Descricao = inputDescricao,
                ItemCatalogoId = inputCatalogo,
                ItemRelacionadoId = (Guid?)null,//
                Tags = "",//
                SolicitanteNome = "Thiago Fernandes",//
                SolicitanteEmail = "thiago.fernades@lucere.com.br"//
            }
        };

        using var httpClient = new HttpClient();

        if (!string.IsNullOrEmpty(tokenUsuario))
        {
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenUsuario}");
        }

        if (!string.IsNullOrEmpty(sistemaId))
        {
            httpClient.DefaultRequestHeaders.Add("Sistema-Id", sistemaId);
        }

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(webhookUrl, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            Message.Set(context, $"Sucesso|Detalhes da resposta: Status =>{response.StatusCode} | body => {responseBody}");
        }
        else
        {
            Message.Set(context, $"Erro: statuscode=>{response.StatusCode} |error => {responseBody}");
        }

        await context.CompleteActivityAsync();
    }

    
}