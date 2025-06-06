using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using System.Text;
using System.Text.Json;
using Elsa.Extensions;

namespace Elsa.Server.Web.Customs.Forms;

[Activity("Forms (NPS)", "Envia um formulario NPS", DisplayName = "Enviar formulário NPS")]
public class EnviarFormulario : Activity
{
    public Output<string> Message { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var config = context.WorkflowExecutionContext.ServiceProvider.GetRequiredService<IConfiguration>();
        var httpClientFactory = context.WorkflowExecutionContext.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        var urlApiForm = $"{config["FormNpsUrl"]}api/formulario/criar-mensagem";

        string? tokenUsuario = context.GetVariable<string>("TokenUsuario");

        var requestBody = new
        {
            formularioId = "1bae8500-2c73-4439-b44d-b68a76d98b86",
            para = "thiago.fernandes@t3p.com.br",
            assunto = "Pesquisa de Satisfação sobre o Atendimento — Lucere Desk",
            corpo = "Pesquisa de Satisfação sobre o Atendimento — Lucere Desk",
            grupo = "Email"
        };

        try
        {
            using var httpClient = httpClientFactory.CreateClient();

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenUsuario}");

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(urlApiForm, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Message.Set(context, $"Sucesso|Detalhes da resposta: Status =>{response.StatusCode} | body => {responseBody}");
            }
            else
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Message.Set(context, $"Erro ao enviar formuário: statuscode=>{response.StatusCode} |error => {responseBody}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exceção: {ex.Message}");
        }

        await context.CompleteActivityAsync();
    }

}
