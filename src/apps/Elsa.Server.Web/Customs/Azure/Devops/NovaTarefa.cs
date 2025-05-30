using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Elsa.Extensions;

namespace Elsa.Server.Web.Customs.Azure.Devops;

[Activity("Azure DevOps", "Cria uma nova tarefa", DisplayName = "Nova tarefa")]
public class NovaTarefa : Activity
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
            string? id = dict["ChamadoId"]?.ToString();
            string? titulo = dict["Titulo"]?.ToString();
            string? descricao = dict["Descricao"]?.ToString();
            string? numero = dict["Numero"]?.ToString();
            DateTime? prazo = dict["Prazo"] != null && DateTime.TryParse(dict["Prazo"].ToString(), out var date) ? date : null;

            var response = await CreateDevopsIntegrationAsync(id, titulo, descricao, numero, prazo);

            if (response > 0)
            {
                Message.Set(context, "Tarefa criada com sucesso.");
            }
            else
            {
                Message.Set(context, "Falha ao criar tarefa no DevOps");
            }
        }

        await context.CompleteActivityAsync();
    }

    private async Task<int> CreateDevopsIntegrationAsync(string? id, string? titulo, string? descricao, string? numero, DateTime? prazo)
    {
        const string organization = "lucere-dev";
        const string project = "Lucere Desk";
        const string personalAccessToken = "5vbqFH5GztruJYmWJwiYjsF0QU8SpuQXY7jqrVed3qVlbafCjgM1JQQJ99BEACAAAAAjHAhOAAASAZDO1cMl";
        const int parentId = 5770;
        const string workItemType = "Task";

        var description = $"<a href='luceredesk-v2-dev.azurewebsites.net/tickets/edit/{id}' target='_blank'>LINK PARA O CHAMADO NO LUCEREDESK</a>";
        var url = $"https://dev.azure.com/{organization}/{project}/_apis/wit/workitems/${workItemType}?api-version=7.0";

        var workItemFields = CreateWorkItemFields(titulo, description, prazo, numero, organization, project, parentId);
        var jsonContent = JsonSerializer.Serialize(workItemFields);

        return await CreateTaskIntegrationAsync(url, jsonContent, personalAccessToken);
    }

    private static List<object> CreateWorkItemFields(string? titulo, string description, DateTime? prazo, string? numero, string organization, string project, int parentId)
    {
        var fields = new List<object>
        {
            new { op = "add", path = "/fields/System.Title", value = titulo },
            new { op = "add", path = "/fields/System.Description", value = description },
            new { op = "add", path = "/fields/Microsoft.VSTS.Scheduling.DueDate", value = prazo?.ToString("yyyy-MM-ddTHH:mm:ssZ") },
            new { op = "add", path = "/fields/Custom.SharePointId", value = numero },
            new
            {
                op = "add",
                path = "/relations/-",
                value = new
                {
                    rel = "System.LinkTypes.Hierarchy-Reverse",
                    url = $"https://dev.azure.com/{organization}/{project}/_apis/wit/workItems/{parentId}",
                    attributes = new
                    {
                        isLocked = false,
                        name = "Parent"
                    }
                }
            }
        };

        return fields;
    }

    private static async Task<int> CreateTaskIntegrationAsync(string url, string jsonContent, string personalAccessToken)
    {
        try
        {
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json-patch+json");
            using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };

            using var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Erro na requisição: {response.StatusCode} - {responseContent}");
                return 0;
            }

            using var jsonDocument = JsonDocument.Parse(responseContent);
            if (jsonDocument.RootElement.TryGetProperty("id", out var idElement) && idElement.TryGetInt32(out var id))
            {
                return id;
            }

            Console.WriteLine("Resposta não contém o campo 'id' esperado");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao criar tarefa: {ex.Message}");
            return 0;
        }
    }
}
