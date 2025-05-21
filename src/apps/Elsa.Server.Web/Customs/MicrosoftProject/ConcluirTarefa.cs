using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Elsa.Extensions;

namespace Elsa.Server.Web.Customs.MicrosoftProject;

[Activity("Microsoft Project", "Concluir Tarefa", DisplayName = "Concluir Tarefa")]
public class ConcluirTarefa : Activity
{
    public Output<string> Message { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        Message.Set(context, "Responsável atribuido");
        await context.CompleteActivityAsync();
    }
}
