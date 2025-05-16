using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SemanticKernelPlayground.Logic
{
    public static class CommitsHelper
    {
        public static async Task<int> GetCommitsCountAsync(Kernel kernel, string input, int defaultCount = 3)
        {
            try
            {
                var chatService = kernel.GetRequiredService<IChatCompletionService>();
                if (chatService == null)
                {
                    Console.WriteLine("Chat service not available, using default count");
                    return defaultCount;
                }

                var systemPrompt = @"You are a number extractor that ONLY outputs a single integer.
Your task is to extract the number of commits the user wants to see from their input.
If no specific number is mentioned, output 3.
ONLY output the number, nothing else.";

                var chatHistory = new ChatHistory(systemPrompt);
                chatHistory.AddUserMessage(input);

                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                var response = await chatService.GetChatMessageContentAsync(
                    chatHistory,
                    executionSettings: new OpenAIPromptExecutionSettings
                    {
                        MaxTokens = 10,
                        Temperature = 0.0
                    }
                );

                if (response == null || string.IsNullOrWhiteSpace(response.Content))
                {
                    Console.WriteLine("AI returned empty response, using default count");
                    return defaultCount;
                }

                var responseText = response.Content.Trim();


                if (int.TryParse(responseText, out var count) && count > 0)
                {
                    return Math.Min(count, 20);
                }

                var digits = new string(responseText.Where(char.IsDigit).ToArray());
                if (!string.IsNullOrEmpty(digits) && int.TryParse(digits, out var extractedCount) && extractedCount > 0)
                {
                    return Math.Min(extractedCount, 20);
                }

                Console.WriteLine($"Could not extract number from AI response: '{responseText}'");
                return defaultCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting commit count: {ex.Message}");
                return defaultCount;
            }
        }
    }
}