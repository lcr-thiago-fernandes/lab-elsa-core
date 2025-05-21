using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Elsa.Extensions;

namespace Elsa.Server.Web.Customs.MicrosoftPlanner;

[Activity("Microsoft Planner", "Atribuir Responsavel Tarefa", DisplayName = "Atribuir Responsável Tarefa")]
public class AtribuirResponsavelTarefaPlanner : Activity
{
    public Output<string> Message { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        Message.Set(context, "Responsável atribuido");
        await context.CompleteActivityAsync();
    }
}
