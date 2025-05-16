using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;


namespace SemanticKernelPlayground.Setup;

public class SetupKernel
{
    public static Kernel CreateKernel(IConfiguration configuration)
    {
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        var modelName = configuration["ModelName"] ?? throw new ApplicationException("ModelName not found");
        var embedding = configuration["EmbeddingModel"] ?? throw new ApplicationException("ModelName not found");
        var endpoint = configuration["Endpoint"] ?? throw new ApplicationException("Endpoint not found");
        var apiKey = configuration["ApiKey"] ?? throw new ApplicationException("ApiKey not found");

        var builder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(modelName, endpoint, apiKey)
            .AddAzureOpenAITextEmbeddingGeneration(embedding, endpoint, apiKey)
            .AddInMemoryVectorStore();

        AddPlugins.Register(builder);
        return builder.Build();
    }
}
