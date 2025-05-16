using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelPlayground.Enums;
using SemanticKernelPlayground.Logic;
using SemanticKernelPlayground.Setup;

namespace SemanticKernelPlayground.Logic
{
    internal static class DetectMode
    {
        public static async Task<AppMode> DetectModeAsync(Kernel kernel, string input)
        {
            try
            {
                var chatService = kernel.GetRequiredService<IChatCompletionService>();
                var arguments = new KernelArguments
                {
                    ["input"] = input
                };
                if (chatService != null)
                {
                    arguments["chatService"] = chatService;
                }
                var result = await kernel.InvokeAsync("ModeDetection", "detect_mode", arguments);
                var modeText = result.ToString().Trim().ToLower();

                
                return modeText switch
                {
                    "commits" => AppMode.Commits,
                    "documentation" => AppMode.Documentation,
                    _ => AppMode.Chat
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DetectModeAsync: {ex.Message}");
                return AppMode.Chat; 
            }
        }
    }
}