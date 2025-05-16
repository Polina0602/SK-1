using Microsoft.Extensions.VectorData;
using System;
using System.Collections.Generic;

namespace SemanticKernelPlayground.Models
{
    public class DocumentationChunk
    {
        [VectorStoreRecordKey] 
        public string Id { get; set; }

        [VectorStoreRecordData] 
        public string Content { get; set; }

        [VectorStoreRecordData] 
        public DocumentMetadata Metadata { get; set; }

        [VectorStoreRecordVector(3072)] 
        public ReadOnlyMemory<float> Vectors { get; set; }

    }

    public class DocumentMetadata
    {
        public string Type { get; set; }
        public string FileName { get; set; }
        public string ElementName { get; set; }
        public string Namespace { get; set; }
        public DateTime LastModified { get; set; }
        public List<string> Tags { get; set; }
    }
}