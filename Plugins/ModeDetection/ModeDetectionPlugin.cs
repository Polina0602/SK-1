using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SemanticKernelPlayground.Plugins.ModeDetection
{
    public class ModeDetectionPlugin
    {
        [KernelFunction("detect_mode")]
        [Description("Detects whether the user wants to chat, work with commits, or search documentation")]
        public async Task<string> DetectMode(
            string input,
            [Description("AI chat completion service")] IChatCompletionService chatService)
        {
            try
            {
                if (chatService == null)
                {
                    Console.WriteLine("Chat service not available, using fallback detection");
                    return "chat";
                }

                var systemPrompt = @"You will be given a user input.
Your task is to answer with ONLY ONE word: 'chat', 'commits', or 'documentation'.
Rules:
- If the input asks to generate release notes, view commits, or mentions versioning — respond with: commits
- If the input asks about code structure, searching documentation, or understanding code — respond with: documentation
- If the input is anything else — respond with: chat
Do not explain. Do not add punctuation. Only answer with exactly one of these three words.";

                var chatHistory = new ChatHistory(systemPrompt);
                chatHistory.AddUserMessage(input);

                var response = await chatService.GetChatMessageContentAsync(
                    chatHistory,
                    executionSettings: new OpenAIPromptExecutionSettings
                    {
                        MaxTokens = 10,
                        Temperature = 0.0
                    }
                );

                var modeText = response.Content?.Trim().ToLower() ?? "chat";

                
                if (modeText != "chat" && modeText != "commits" && modeText != "documentation")
                {
                    Console.WriteLine($"AI returned invalid result: '{modeText}', using fallback detection");
                    return "chat";
                }

                return modeText;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DetectMode: {ex.Message}");
                return "chat";
            }
        }
    }
}