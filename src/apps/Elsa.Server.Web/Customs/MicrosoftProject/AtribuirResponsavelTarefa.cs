using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Elsa.Extensions;

namespace Elsa.Server.Web.Customs.MicrosoftProject;

[Activity("Microsoft Project", "Atribuir Responsavel Tarefa", DisplayName = "Atribuir Responsável Tarefa")]
public class AtribuirResponsavelTarefa : Activity
{
    public Output<string> Message { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        Message.Set(context, "Responsável atribuído");
        await context.CompleteActivityAsync();
    }
}
