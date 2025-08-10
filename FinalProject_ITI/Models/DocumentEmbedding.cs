using System;

namespace FinalProject_ITI.Models
{
    public class DocumentEmbedding
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string EntityType { get; set; } = string.Empty; // e.g. "Product", "Brand", ...
        public string EntityId { get; set; } = string.Empty;   // e.g. product.Id.ToString()
        public string Source { get; set; } = string.Empty;     // human readable source
        public string Content { get; set; } = string.Empty;    // text used to create embedding (chunk or full)
        public byte[]? Embedding { get; set; }                 // varbinary(max)
        public int Dimension { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
