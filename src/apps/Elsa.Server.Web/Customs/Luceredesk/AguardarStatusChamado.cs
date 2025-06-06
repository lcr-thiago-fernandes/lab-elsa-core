using System.Text;
using System.Text.Json;
using Elastic.Clients.Elasticsearch.MachineLearning;
using Elsa.Extensions;
using Elsa.Http;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;

namespace Elsa.Server.Web.Customs.Luceredesk;

[Activity("Lucere", "Aguarda a o status do chamado", DisplayName = "Aguardar alteração de status chamado")]
public class AguardarStatusChamado : HttpEndpointBase
{

    [Input(
        Description = "Descrição do status que deseja aguardar",
        Category = "Configuration"
    )]
    public Input<string> StatusNome { get; set; } = null!;

    protected override HttpEndpointOptions GetOptions()
    {
        return new()
        {
            //Path = "chamado/{0}/alteracao-status",
            Path = "chamado/alteracao-status",
            Methods = [HttpMethods.Post],
            Authorize = true
        };
    }

    //protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    //{
    //    var options = GetOptions();
    //    var chamadoId = context.GetWorkflowInput<string>("ChamadoId");
    //    options.Path = string.Format(options.Path, chamadoId);
    //    context.WaitForHttpRequest(options);
    //}

    protected override async ValueTask OnHttpRequestReceivedAsync(ActivityExecutionContext context, HttpContext httpContext)
    {
        httpContext.Response.StatusCode = 200;

        var inputStatusValue = (string)StatusNome.Expression.Value;

        httpContext.Request.EnableBuffering();
        using var reader = new StreamReader(httpContext.Request.Body, encoding: Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        httpContext.Request.Body.Position = 0;

        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        if (root.TryGetProperty("TokenUsuario", out var tokenUsuario))
        {
            context.SetVariable("TokenUsuario", tokenUsuario.GetString());
        }

        if (root.TryGetProperty("statusNome", out var statusNomeElement))
        {
            var statusNome = statusNomeElement.GetString();

            if (string.Equals(inputStatusValue, statusNome, StringComparison.CurrentCultureIgnoreCase))
            {
                await context.CompleteActivityAsync();
            }
            else
            {
                context.TransitionTo(ActivityStatus.Pending);
                return;
            }
        }
    }
}
