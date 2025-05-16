using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticKernelPlayground.Enums;
using SemanticKernelPlayground.Logic;
using SemanticKernelPlayground.Plugins.ModeDetection;
using SemanticKernelPlayground.Setup;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
    .Build();

var kernel = SetupKernel.CreateKernel(configuration);
var chatService = kernel.GetRequiredService<IChatCompletionService>();
var history = new ChatHistory();

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("┌────────────────────────────────────────────┐");
Console.WriteLine("│       Welcome to Semantic Kernel AI!       │");
Console.WriteLine("└────────────────────────────────────────────┘");
Console.ResetColor();
Console.WriteLine();
Console.WriteLine("I can help you with three main things:");

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Git & Release Notes");
Console.ResetColor();
Console.WriteLine("   • Generate release notes from commits");
Console.WriteLine("   • View recent repository activity");
Console.WriteLine("   • Just mention 'commits' or 'release notes'");
Console.WriteLine();

Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("Code Documentation");
Console.ResetColor();
Console.WriteLine("   • Search through your code documentation");
Console.WriteLine("   • Ask questions about your code base");
Console.WriteLine("   • Just mention 'code', 'documentation', or 'search'");
Console.WriteLine();

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("Chat with AI");
Console.ResetColor();
Console.WriteLine("   • Ask me any question");
Console.WriteLine("   • Get help with coding, learning, or ideas");
Console.WriteLine("   • Just type your message to start chatting");
Console.WriteLine();

Console.Write("What would you like to do today? ");
var input = Console.ReadLine();
if (input?.Trim().ToLower() == "exit") return;

var mode = await DetectMode.DetectModeAsync(kernel, input);

switch (mode)
{
    case AppMode.Commits:
        await RunCommitsMode.RunCommitsModeAsync(kernel, chatService, history, input!);
        break;

    case AppMode.Documentation:
        await RunDocumentationMode.RunDocumentationModeAsync(kernel, chatService, history, input!);
        break;

    case AppMode.Chat:
    default:
        await RunChatMode.RunChatModeAsync(kernel, chatService, history, input!);
        break;
}