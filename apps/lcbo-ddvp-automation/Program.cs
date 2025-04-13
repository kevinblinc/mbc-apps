using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using LcboDdvpAutomation;

class Program
{
    static async Task Main(string[] args)
    {
        Func<ILambdaContext, Task> handler = async context =>
        {
            await new Function().FunctionHandlerAsync(context);
        };

        await LambdaBootstrapBuilder.Create(handler)
            .Build()
            .RunAsync();
    }
}
