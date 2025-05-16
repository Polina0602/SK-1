using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using SemanticKernelPlayground.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace SemanticKernelPlayground.Services;

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
public class VectorStoreService
{
    private readonly IVectorStore _vectorStore;
    private readonly ITextEmbeddingGenerationService _embeddingGenerator;

    public VectorStoreService(IVectorStore vectorStore, ITextEmbeddingGenerationService embeddingGenerator)
    {
        _vectorStore = vectorStore;
        _embeddingGenerator = embeddingGenerator;
    }

    private IVectorStoreRecordCollection<string, DocumentationChunk> GetCollection(string collectionName)
    {
        var collection = _vectorStore.GetCollection<string, DocumentationChunk>(collectionName);

        return collection;
    }

    public async Task IngestDocumentationAsync(string collectionName, IEnumerable<DocumentationChunk> documents)
    {
        try
        {
            Console.WriteLine($"Starting batch ingestion of {documents.Count()} documents into collection '{collectionName}'");


            var collection = _vectorStore.GetCollection<string, DocumentationChunk>(collectionName);


            await collection.CreateCollectionIfNotExistsAsync();


            int processedCount = 0;

            foreach (var doc in documents)
            {

                await collection.UpsertAsync(doc);

                processedCount++;
                if (processedCount % 10 == 0)
                {
                    Console.WriteLine($"Ingestion progress: {processedCount}/{documents.Count()} documents processed");
                }
            }

            Console.WriteLine($"Batch ingestion completed. {processedCount} documents loaded into vector store.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during batch ingestion: {ex.Message}");
            throw;
        }
    }

    public async Task<IReadOnlyList<DocumentationChunk>> SearchDocumentationAsync(string collectionName, string query, int limit = 5)
    {
        try
        {
            var collection = _vectorStore.GetCollection<string, DocumentationChunk>(collectionName);

            var queryEmbedding = await _embeddingGenerator.GenerateEmbeddingAsync(query);

            var searchResults = collection.SearchEmbeddingAsync(queryEmbedding, limit);

            var documents = new List<DocumentationChunk>();

            await foreach (var result in searchResults)
            {
                documents.Add(result.Record);
            }

            return documents;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching documentation: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Saves code documentation to the vector store
    /// </summary>
    public async Task SaveDocumentationAsync(string collectionName, DocumentationChunk docChunk)
    {
        try
        {
            var collection = _vectorStore.GetCollection<string, DocumentationChunk>(collectionName);

            var embedding = await _embeddingGenerator.GenerateEmbeddingAsync(docChunk.Content);
            docChunk.Vectors = embedding;

            await collection.CreateCollectionIfNotExistsAsync();

            await collection.UpsertAsync(docChunk);


        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving documentation: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Alias method for compatibility with the CodeAnalysisPlugin
    /// </summary>
    public Task SaveDocumentationChunkAsync(string collectionName, DocumentationChunk docChunk)
    {
        return SaveDocumentationAsync(collectionName, docChunk);
    }

    /// <summary>
    /// Saves a set of documents to the vector store
    /// </summary>
    public async Task SaveDocumentsAsync(string collectionName, IEnumerable<DocumentationChunk> documents)
    {
        foreach (var doc in documents)
        {
            await SaveDocumentationAsync(collectionName, doc);
        }
    }

    
    /// <summary>
    /// Get document by ID
    /// </summary>
    public async Task<DocumentationChunk> GetDocumentAsync(string collectionName, string id)
    {
        try
        {
            var collection = _vectorStore.GetCollection<string, DocumentationChunk>(collectionName);
            return await collection.GetAsync(id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving document: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Delete document by ID
    /// </summary>
    public async Task DeleteDocumentAsync(string collectionName, string id)
    {
        try
        {
            var collection = _vectorStore.GetCollection<string, DocumentationChunk>(collectionName);
            await collection.DeleteAsync(id);
            Console.WriteLine($"Document with ID {id} deleted");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting document: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Clear the entire collection
    /// </summary>
    public async Task ClearCollectionAsync(string collectionName)
    {
        try
        {
            // Get the collection
            var collection = _vectorStore.GetCollection<string, DocumentationChunk>(collectionName);

            // Delete the collection
            await collection.DeleteCollectionAsync();
            Console.WriteLine($"Deleted collection '{collectionName}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing collection: {ex.Message}");
            throw;
        }
    }

 

    /// <summary>
    /// Create a new collection if it doesn't exist
    /// </summary>
    public async Task CreateCollectionIfNotExistsAsync(string collectionName)
    {
        var collection = _vectorStore.GetCollection<string, DocumentationChunk>(collectionName);
        await collection.CreateCollectionIfNotExistsAsync();
        Console.WriteLine($"Ensured collection '{collectionName}' exists");
    }
}
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
