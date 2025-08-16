using System;

namespace FinalProject_ITI.Models
{
    public class ContextChunk
    {
        public Guid DocumentId { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public float Score { get; set; }
        public int Dimension { get; set; }
    }
}
