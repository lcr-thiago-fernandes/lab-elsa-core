using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using System.Text;
using System.Text.Json;

namespace Elsa.Server.Web.Customs.Forms;

[Activity("Forms (NPS)", "Envia um formulario NPS", DisplayName = "Enviar formulário NPS")]
public class EnviarFormulario : Activity
{
    public Output<string> Message { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var config = context.WorkflowExecutionContext.ServiceProvider.GetRequiredService<IConfiguration>();
        var httpClientFactory = context.WorkflowExecutionContext.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        var urlApiForm = $"{config["FormNpsUrl"]}/api/formulario/criar-mensagem";

        /* ?passar token no contexto*/
        var bearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VyTmFtZSI6Imd1aWxoZXJtZS5tYW50ekBsdWNlcmUuY29tLmJyIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6Ikd1aWxoZXJtZSBkYSBSb2NoYSBNYW50eiIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiR00iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJndWlsaGVybWUubWFudHpAbHVjZXJlLmNvbS5iciIsIlNpc3RlbWFJZCI6IjQ0OTJiMDcyLWZkZjEtNDY1NS1iZTM4LTJhZmU4YzIwZDlmOCIsIkF1dGhQcm92aWRlciI6Ik1pY3Jvc29mdCIsIlNpc3RlbWFPd25lciI6IjQ0OTJCMDcyLUZERjEtNDY1NS1CRTM4LTJBRkU4QzIwRDlGOCIsImV4cCI6MTc0ODcyNDA0NCwiaXNzIjoibHVjZXJlZGVzayIsImF1ZCI6Imh0dHBzOi8vbG9jYWxob3N0In0.O2dIAUqAY9GY-LXvBAXG8CjthLSUyIJNt4dL5LwZreg";

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

            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer ", bearerToken);

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(urlApiForm, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Sucesso: {responseContent}");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erro: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exceção: {ex.Message}");
        }
    }

}
