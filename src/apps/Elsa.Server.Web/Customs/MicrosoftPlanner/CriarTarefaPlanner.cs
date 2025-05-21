using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Elsa.Extensions;

namespace Elsa.Server.Web.Customs.MicrosoftPlanner;

[Activity("Microsoft Planner", "Criar Tarefa", DisplayName = "Criar Tarefa")]
public class CriarTarefaPlanner : Activity
{
    public Output<string> Message { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        Message.Set(context, "Tarefa Criada");
        await context.CompleteActivityAsync();
    }
}
