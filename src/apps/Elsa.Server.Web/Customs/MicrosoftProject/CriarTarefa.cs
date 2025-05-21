using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Extensions;
using Elsa.Workflows.Models;

namespace Elsa.Server.Web.Customs.MicrosoftProject;

[Activity("Microsoft Project", "Criar Tarefa", DisplayName = "Criar Tarefa")]
public class CriarTarefa : Activity
{
    public Output<string> Message { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        Message.Set(context, "Tarefa Criada");
        await context.CompleteActivityAsync();
    }
}
