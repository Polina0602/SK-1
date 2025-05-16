using Microsoft.SemanticKernel;
using SemanticKernelPlayground.Models;
using SemanticKernelPlayground.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SemanticKernelPlayground.Plugins;
    internal class CodeAnalysisPlugin
    {
        private readonly VectorStoreService _vectorStoreService;

        public CodeAnalysisPlugin(VectorStoreService vectorStoreService)
        {
            _vectorStoreService = vectorStoreService;
        }

    [KernelFunction]
    [Description("Ingests all code files from a directory into the vector store")]
    public async Task<string> IngestCodeDirectory(
    string directoryPath,
    [Description("File extensions to include, e.g. '.cs,.js'")]
    string extensions = ".cs",
    [Description("Collection name to use")]
    string collectionName = "code_documentation")

    {
        
        if (!Directory.Exists(directoryPath))
        {
            return $"Directory not found: {directoryPath}";
        }

        var extensionArray = extensions.Split(',').Select(e => e.Trim()).ToArray();
        var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                             .Where(f => extensionArray.Any(e => f.EndsWith(e)));

        Console.WriteLine($"Found {files.Count()} files to process in directory: {directoryPath}");

        List<DocumentationChunk> allChunks = new List<DocumentationChunk>();

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            var relativeFilePath = file.Substring(directoryPath.Length).TrimStart('\\', '/');
            var content = await File.ReadAllTextAsync(file);


            var chunks = AnalyzeCodeFile(content, relativeFilePath, fileInfo.Extension);
            allChunks.AddRange(chunks);
        }

        Console.WriteLine($"Analysis complete. Generated {allChunks.Count} documentation chunks.");
        Console.WriteLine("Starting vector store ingestion...");

        await _vectorStoreService.IngestDocumentationAsync(collectionName, allChunks);

        return $"Successfully ingested {allChunks.Count} documentation chunks from {files.Count()} files into collection '{collectionName}'.";
    }

    [KernelFunction]
        [Description("Scans a directory for code files and analyzes them")]
        public async Task<string> ScanDirectory(
    string directoryPath,
    [Description("File extensions to include, e.g. '.cs,.js'")]
    string extensions = ".cs")
        {

            if (!Directory.Exists(directoryPath))
            {
                return $"Directory not found: {directoryPath}";
            }

            var extensionArray = extensions.Split(',').Select(e => e.Trim()).ToArray();

            var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                                .Where(f => extensionArray.Any(e => f.EndsWith(e)));


            int processedFiles = 0;
            int generatedChunks = 0;


            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var relativeFilePath = file.Substring(directoryPath.Length).TrimStart('\\', '/');


                var content = await File.ReadAllTextAsync(file);


                var chunks = AnalyzeCodeFile(content, relativeFilePath, fileInfo.Extension);


                foreach (var chunk in chunks)
                {
                    await _vectorStoreService.SaveDocumentationChunkAsync("code_documentation", chunk);
                    generatedChunks++;
                }

                processedFiles++;
            }

        return $"Successfully Processed {processedFiles} files, generated {generatedChunks} documentation chunks.";
        }

        private List<DocumentationChunk> AnalyzeCodeFile(string content, string filePath, string extension)
        {
            var chunks = new List<DocumentationChunk>();


            if (extension == ".cs") 
            {

                var classMatches = Regex.Matches(content, @"class\s+(\w+)");
                foreach (Match match in classMatches)
                {
                    var className = match.Groups[1].Value;
                    var startIndex = match.Index;

                    var classEndIndex = content.IndexOf("}", startIndex);
                    if (classEndIndex > startIndex)
                    {
                        var classContent = content.Substring(startIndex, classEndIndex - startIndex + 1);

                        chunks.Add(new DocumentationChunk
                        {
                            Id = Guid.NewGuid().ToString(),
                            Content = $"Class: {className}\n\n{classContent}",
                            Metadata = new DocumentMetadata
                            {
                                Type = "Class",
                                FileName = filePath,
                                ElementName = className,
                                LastModified = DateTime.Now,
                                Tags = new List<string> { "class", className }
                            }
                        });
                    }
                }
            }

            if (chunks.Count == 0)
            {
                chunks.Add(new DocumentationChunk
                {
                    Id = Guid.NewGuid().ToString(),
                    Content = content,
                    Metadata = new DocumentMetadata
                    {
                        Type = "File",
                        FileName = filePath,
                        ElementName = Path.GetFileName(filePath),
                        LastModified = DateTime.Now,
                        Tags = new List<string> { "file", extension }
                    }
                });
            }

            return chunks;
        }
    }

