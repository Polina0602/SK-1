using System;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticKernelPlayground.Models;
using SemanticKernelPlayground.Services;

namespace SemanticKernelPlayground.Plugins
{
    public class MemoryPlugin
    {
        private readonly VectorStoreService _vectorStoreService;
        private readonly IChatCompletionService _chatService;

        public MemoryPlugin(VectorStoreService vectorStoreService, IChatCompletionService chatService)
        {
            _vectorStoreService = vectorStoreService;
            _chatService = chatService;
        }

        [KernelFunction]
        [Description("Searches the documentation for relevant information")]
        public async Task<string> SearchDocumentation(string query, int limit = 5)
        {
            var results = await _vectorStoreService.SearchDocumentationAsync("code_documentation", query, limit);

            if (results.Count == 0)
            {
                return "No relevant documentation found.";
            }

            var formattedResults = new StringBuilder();
            formattedResults.AppendLine($"Found {results.Count} relevant document(s):\n");

            foreach (var doc in results)
            {
                formattedResults.AppendLine($"## {doc.Metadata?.Type}: {doc.Metadata?.ElementName}");
                formattedResults.AppendLine($"File: {doc.Metadata?.FileName}");

                if (doc.Metadata?.Tags != null && doc.Metadata.Tags.Count > 0)
                {
                    formattedResults.AppendLine($"Tags: {string.Join(", ", doc.Metadata.Tags)}");
                }

                formattedResults.AppendLine();
                formattedResults.AppendLine("```");
                var contentPreview = doc.Content?.Length > 500
                    ? doc.Content.Substring(0, 500) + "..."
                    : doc.Content;
                formattedResults.AppendLine(contentPreview);
                formattedResults.AppendLine("```");
                formattedResults.AppendLine();
            }

            return formattedResults.ToString();
        }

        [KernelFunction]
        [Description("Answers a question about the code using the documentation")]
        public async Task<string> AnswerQuestion(string question, int contextLimit = 3)
        {
            // Находим релевантную документацию
            var results = await _vectorStoreService.SearchDocumentationAsync("code_documentation", question, contextLimit);

            if (results.Count == 0)
            {
                return "I don't have enough information to answer this question based on the available documentation.";
            }

            var context = new StringBuilder();
            context.AppendLine("Based on the following code documentation:");
            context.AppendLine();

            foreach (var doc in results)
            {
                context.AppendLine($"--- {doc.Metadata?.Type}: {doc.Metadata?.ElementName} ({doc.Metadata?.FileName}) ---");
                context.AppendLine(doc.Content);
                context.AppendLine();
            }

            var prompt = $@"You are an assistant that helps developers understand their code.
Based on the following code documentation, please answer the question:
{context}
Question: {question}
Provide a clear and concise answer based only on the information in the documentation.
If the documentation doesn't contain relevant information, say so.";

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(prompt);
            var response = await _chatService.GetChatMessageContentAsync(chatHistory);

            return response.Content;
        }
    }
}