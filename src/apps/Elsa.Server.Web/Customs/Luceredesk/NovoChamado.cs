using Elsa.Workflows.Attributes;
using Elsa.Workflows;
using System.Text;
using System.Text.Json;
using Elsa.Workflows.Models;
using Elsa.Extensions;

namespace Elsa.Server.Web.Customs.Luceredesk;

[Activity("Lucere", "Novo chamado", DisplayName = "Novo Chamado")]
public class NovoChamado : Activity
{
    public Output<string> Message { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {

        var config = context.WorkflowExecutionContext.ServiceProvider.GetRequiredService<IConfiguration>();

        var webhookUrl = config.GetSection("Webhooks:Sinks")
            .GetChildren()
            .FirstOrDefault(s => s["Id"] == "1")?["Url"];

        var parametros = context.WorkflowInput["Parametros"];

        if (parametros is IDictionary<string, object> dict)
        {
            string? tokenUsuario = context.WorkflowInput["TokenUsuario"]?.ToString();
            string? sistemaId = context.WorkflowInput["SistemaId"]?.ToString();

            var payload = new
            {
                Event = "NovoChamado",
                Chamado = new
                {
                    TipoId = Guid.Parse("b86633d0-a6c2-4c0a-af4c-6900ef81303a"),
                    Numero = 159753,
                    Backlog = true,
                    Titulo = "Título de exemplo",
                    Descricao = "Descrição do chamado",
                    ItemCatalogoId = Guid.Parse("6034b604-8845-447c-97ec-5fd115c53071"),
                    ItemRelacionadoId = (Guid?)null,
                    Tags = "urgente,prioridade",
                    SolicitanteNome = "Guilherme",
                    SolicitanteEmail = "guilherme@empresa.com"
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

            if (response.IsSuccessStatusCode)
            {
                Message.Set(context, "Chamado criado com sucesso.");
            }
            else
            {
                Message.Set(context, "Erro ao criar chamado");
            }
        }

        await context.CompleteActivityAsync();
    }
}
