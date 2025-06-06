using System.Text;
using System.Text.Json;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Elsa.Extensions;

namespace Elsa.Server.Web.Customs.Luceredesk;

[Activity("Lucere", "Alterar situação do chamado", DisplayName = "Alterar situação Chamado")]
public class AlterarSituacaoChamado : Activity
{

    public Output<string> Message { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var config = context.WorkflowExecutionContext.ServiceProvider.GetRequiredService<IConfiguration>();

        var webhookUrl = config.GetSection("Webhooks:Sinks")
            .GetChildren()
            .FirstOrDefault(s => s["Id"] == "1")?["Url"];

        var parametros = context.WorkflowInput["Parametros"];

        if(parametros is IDictionary<string, object> dict)
        {
            string? chamadoId = dict["ChamadoId"]?.ToString();
            string? situacaoId = dict["SituacaoId"]?.ToString();
            string? tokenUsuario = context.WorkflowInput["TokenUsuario"]?.ToString();
            string? sistemaId = context.WorkflowInput["SistemaId"]?.ToString();

            var payload = new
            {
                Event = "AlterarSituacaoChamado",
                ChamadoId = chamadoId,
                SituacaoId = situacaoId
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
        }

        await context.CompleteActivityAsync();
    }
}
