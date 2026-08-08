using System;

namespace VoiceBookStudio.Models
{
    /// <summary>One user message + the AI's response, stored as a single history entry
    /// so selecting a past exchange shows the full back-and-forth, not just one side.</summary>
    public class ChatExchange
    {
        public string   Id          { get; set; } = Guid.NewGuid().ToString();
        public string   UserMessage { get; set; } = string.Empty;
        public string   AiResponse  { get; set; } = string.Empty;
        public DateTime CreatedAt   { get; set; } = DateTime.Now;
    }
}
