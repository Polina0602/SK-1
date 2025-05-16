using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace SemanticKernelPlayground.Plugins.ReleaseNotes
{
    public class ReleaseNotesPlugin
    {
        private static string _cachedPrompt;

        [KernelFunction("generate")]
        [Description("Generates release notes from git commits")]
        public async Task<string> GenerateReleaseNotes(
            string commits,
            [Description("AI service")] IChatCompletionService chatService)
        {
            try
            {
                if (chatService == null)
                {
                    return "AI service not available. Cannot generate release notes.";
                }

                string systemPrompt = GetReleaseNotesPrompt();

                systemPrompt = systemPrompt.Replace("{{$commits}}", commits);

                var chatHistory = new ChatHistory(systemPrompt);
                chatHistory.AddUserMessage($"Here are the commits to summarize into release notes:\n\n{commits}");

                var response = await chatService.GetChatMessageContentAsync(chatHistory);
                return response.Content ?? "Failed to generate release notes.";
            }
            catch (Exception ex)
            {
                return $"Error generating release notes: {ex.Message}";
            }
        }

        private string GetReleaseNotesPrompt()
        {
            var baseDir = AppContext.BaseDirectory;
            var _promptPath = Path.Combine(baseDir, "Plugins", "ReleaseNotes", "ReleaseNotes.prompt");

            if (_cachedPrompt != null)
            {
                return _cachedPrompt;
            }

            string fileContent = File.ReadAllText(_promptPath);

            if (fileContent.StartsWith("name:") || fileContent.StartsWith("---"))
            {
                int templateIndex = fileContent.IndexOf("template: |");
                if (templateIndex >= 0)
                {
                    templateIndex += "template: |".Length;
                    fileContent = fileContent.Substring(templateIndex).Trim();
                }
            }
            _cachedPrompt = fileContent;
            return _cachedPrompt;
        }
    }
}

