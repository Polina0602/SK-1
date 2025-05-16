using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;


namespace SemanticKernelPlayground.Logic
{
    internal class RunChatMode
    {
        public static async Task RunChatModeAsync(Kernel kernel, IChatCompletionService chatService, ChatHistory history, string input)
        {
            history.AddUserMessage(input);
            var settings = new AzureOpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            var fullResponse = "";
            await foreach (var chunk in chatService.GetStreamingChatMessageContentsAsync(history, settings, kernel))
            {
                if (!string.IsNullOrEmpty(chunk.Content))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(chunk.Content);
                    Console.ResetColor();
                    fullResponse += chunk.Content;
                }
            }

            Console.WriteLine();
            history.AddAssistantMessage(fullResponse);


            do
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("You > ");
                Console.ResetColor();

                var userInput = Console.ReadLine();
                if (userInput?.Trim().ToLower() == "exit") break;

                history.AddUserMessage(userInput!);

                fullResponse = "";

                await foreach (var chunk in chatService.GetStreamingChatMessageContentsAsync(history, settings, kernel))
                {
                    if (!string.IsNullOrEmpty(chunk.Content))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(chunk.Content);
                        Console.ResetColor();
                        fullResponse += chunk.Content;
                    }
                }

                Console.WriteLine();
                history.AddAssistantMessage(fullResponse);
            }
            while (true);
        }
    }
}