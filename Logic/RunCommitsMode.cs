using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;


namespace SemanticKernelPlayground.Logic
{
    internal class RunCommitsMode
    {
        public static async Task RunCommitsModeAsync(Kernel kernel, IChatCompletionService chatService, ChatHistory history, string input)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Enter the path to your Git repository: ");
            Console.ResetColor();
            var repoPath = Console.ReadLine();

            await kernel.InvokeAsync("Git", "SetRepositoryPath", new() { ["path"] = repoPath });

            var detectionResult = await CommitsHelper.GetCommitsCountAsync(kernel, input);
            var countText = detectionResult.ToString().Trim();
            if (!int.TryParse(countText, out var commitsCount) || commitsCount <= 0)
                commitsCount = 3;

            var commits = await kernel.InvokeAsync("Git", "GetLatestCommits", new() { ["count"] = commitsCount.ToString() });

            var commitsData = new KernelArguments
            {
                ["commits"] = commits.GetValue<string>(),
                ["chatService"] = chatService
            };

            var result = await kernel.InvokeAsync("ReleaseNotes", "generate", commitsData);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n Release Notes:");
            Console.WriteLine(result);
            Console.ResetColor();
        }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error in RunCommitsModeAsync: {ex.Message}");
        Console.ResetColor();
    }
}

    }
}
