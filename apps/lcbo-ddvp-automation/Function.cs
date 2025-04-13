using Amazon.Lambda.Core;

namespace LcboDdvpAutomation;

public class Function
{
    public async Task FunctionHandlerAsync(ILambdaContext context)
    {
        context.Logger.LogLine("🚀 LCBO DDVP Automation Lambda executed.");
        await Task.CompletedTask;
    }
}
