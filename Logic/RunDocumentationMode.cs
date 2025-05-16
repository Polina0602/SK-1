using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace SemanticKernelPlayground.Logic
{
    public static class RunDocumentationMode
    {
        public static async Task RunDocumentationModeAsync(
            Kernel kernel,
            IChatCompletionService chatService,
            ChatHistory history,
            string initialQuery)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n=== Documentation Mode ===");
            Console.ResetColor();
            Console.WriteLine("You can:");
            Console.WriteLine("- Ask questions about your code");
            Console.WriteLine("- Search for specific topics in documentation");
            Console.WriteLine("- Scan directories to analyze and document code");
            Console.WriteLine("Type 'exit' to return to the main menu.\n");

            
            await ProcessDocumentationQueryAsync(kernel, initialQuery);

            
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("\nDocumentation> ");
                Console.ResetColor();

                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input) || input.Trim().ToLower() == "exit")
                    break;

                await ProcessDocumentationQueryAsync(kernel, input);
            }
        }

        private static bool IsIngestQuery(string query)
        {
            
            var lowercaseQuery = query.ToLower();
            return lowercaseQuery.Contains("ingest") ||
                   lowercaseQuery.Contains("import") ||
                   lowercaseQuery.Contains("load") ||
                   (lowercaseQuery.Contains("index") && lowercaseQuery.Contains("code"));
        }

        private static async Task ProcessDocumentationQueryAsync(Kernel kernel, string query)
        {
            try
            {
                if (IsIngestQuery(query))
                {
                    
                    var directoryPath = ExtractDirectoryPath(query);
                    var fileExtensions = ExtractFileExtensions(query);

                    Console.WriteLine($"Ingesting code from directory: '{directoryPath}' with extensions: {fileExtensions}");

                    var result = await kernel.InvokeAsync("CodeAnalysis", "IngestCodeDirectory", new KernelArguments
                    {
                        ["directoryPath"] = directoryPath,
                        ["extensions"] = fileExtensions,
                        ["collectionName"] = "code_documentation"
                    });

                    Console.WriteLine(result?.ToString());
                }

                if (IsScanDirectoryQuery(query))
                {
                    
                    var directoryPath = ExtractDirectoryPath(query);
                    var fileExtensions = ExtractFileExtensions(query);

                    Console.WriteLine($"Scanning directory: '{directoryPath}' for files with extensions: {fileExtensions}");

                    var result = await kernel.InvokeAsync("CodeAnalysis", "ScanDirectory", new KernelArguments
                    {
                        ["directoryPath"] = directoryPath,
                        ["extensions"] = fileExtensions
                    });

                    Console.WriteLine(result?.ToString());
                }
                else if (IsSearchQuery(query))
                {
                    
                    var cleanQuery = CleanSearchQuery(query);

                    Console.WriteLine($"Searching documentation for: '{cleanQuery}'");

                    var result = await kernel.InvokeAsync("Memory", "SearchDocumentation", new KernelArguments
                    {
                        ["query"] = cleanQuery,
                        ["limit"] = 5
                    });

                    Console.WriteLine(result?.ToString());
                }
                else if (IsQuestion(query))
                {
                    
                    Console.WriteLine("Analyzing your question...");

                    var result = await kernel.InvokeAsync("Memory", "AnswerQuestion", new KernelArguments
                    {
                        ["question"] = query,
                        ["contextLimit"] = 3
                    });

                    Console.WriteLine(result?.ToString());
                }
                else
                {
                    
                    Console.WriteLine($"Searching documentation for: '{query}'");

                    var result = await kernel.InvokeAsync("Memory", "SearchDocumentation", new KernelArguments
                    {
                        ["query"] = query,
                        ["limit"] = 5
                    });

                    Console.WriteLine(result?.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error processing your query: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static bool IsScanDirectoryQuery(string query)
        {
            
            var lowercaseQuery = query.ToLower();
            return lowercaseQuery.Contains("scan") ||
                   lowercaseQuery.Contains("analyze") ||
                   lowercaseQuery.Contains("parse") ||
                   lowercaseQuery.Contains("document") ||
                   (lowercaseQuery.Contains("directory") &&
                    (lowercaseQuery.Contains("code") || lowercaseQuery.Contains("files")));
        }

        private static string ExtractDirectoryPath(string query)
        {
            
            string[] parts = query.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (parts[i].ToLower() == "directory" || parts[i].ToLower() == "folder" ||
                    parts[i].ToLower() == "path" || parts[i].ToLower() == "scan")
                {
                    return parts[i + 1].Trim('\'', '"', '`');
                }
            }

            
            Console.WriteLine("Please enter the directory path to scan:");
            return Console.ReadLine()?.Trim() ?? ".";
        }

        private static string ExtractFileExtensions(string query)
        {
           
            if (query.Contains(".cs"))
                return ".cs";
            else if (query.Contains(".js"))
                return ".js";
            else if (query.Contains(".py"))
                return ".py";
            else if (query.Contains(".java"))
                return ".java";

            
            return ".cs";
        }

        private static bool IsSearchQuery(string query)
        {
            
            var lowercaseQuery = query.ToLower();
            return lowercaseQuery.Contains("search") ||
                   lowercaseQuery.Contains("find") ||
                   lowercaseQuery.Contains("look for") ||
                   lowercaseQuery.Contains("show me");
        }

        private static bool IsQuestion(string query)
        {
            
            var lowercaseQuery = query.ToLower().Trim();
            return lowercaseQuery.Contains("?") ||
                   lowercaseQuery.StartsWith("how") ||
                   lowercaseQuery.StartsWith("what") ||
                   lowercaseQuery.StartsWith("why") ||
                   lowercaseQuery.StartsWith("when") ||
                   lowercaseQuery.StartsWith("where") ||
                   lowercaseQuery.StartsWith("who") ||
                   lowercaseQuery.StartsWith("which") ||
                   lowercaseQuery.StartsWith("can") ||
                   lowercaseQuery.StartsWith("does") ||
                   lowercaseQuery.StartsWith("do");
        }

        private static string CleanSearchQuery(string query)
        {
            
            return query
                .Replace("search", "", StringComparison.OrdinalIgnoreCase)
                .Replace("find", "", StringComparison.OrdinalIgnoreCase)
                .Replace("look for", "", StringComparison.OrdinalIgnoreCase)
                .Replace("show me", "", StringComparison.OrdinalIgnoreCase)
                .Replace("documentation", "", StringComparison.OrdinalIgnoreCase)
                .Replace("about", "", StringComparison.OrdinalIgnoreCase)
                .Replace("for", "", StringComparison.OrdinalIgnoreCase)
                .Replace("related to", "", StringComparison.OrdinalIgnoreCase)
                .Trim();
        }
    }
}