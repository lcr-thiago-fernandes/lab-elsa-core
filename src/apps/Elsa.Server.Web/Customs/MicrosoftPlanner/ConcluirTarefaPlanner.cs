using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Elsa.Extensions;

namespace Elsa.Server.Web.Customs.MicrosoftPlanner;

[Activity("Microsoft Planner", "Concluir Tarefa", DisplayName = "Concluir Tarefa")]
public class ConcluirTarefaPlanner : Activity
{
    public Output<string> Message { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        Message.Set(context, "Tarefa concluida");
        await context.CompleteActivityAsync();
    }
}
