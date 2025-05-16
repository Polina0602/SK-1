using LibGit2Sharp;
using Microsoft.SemanticKernel;
using System.Text;


namespace SemanticKernelPlayground.Plugins.GitPlugin;

public class GitPlugin
{
    private string? _repositoryPath;

    [KernelFunction]
    public void SetRepositoryPath(string path)
    {
        _repositoryPath = path;
    }

    [KernelFunction]
    public string GetLatestCommits(int count)
    {
        if (string.IsNullOrWhiteSpace(_repositoryPath) || !Repository.IsValid(_repositoryPath))
        {
            return "Invalid or missing repository path.";
        }

        using var repo = new Repository(_repositoryPath);
        var sb = new StringBuilder();

        foreach (var commit in repo.Commits.Take(count))
        {
            sb.AppendLine($"- {commit.MessageShort} ({commit.Author.Name}, {commit.Author.When})");
        }

        return sb.ToString();
    }
}

