using System;

namespace Domain.Entities
{
    public sealed class ErrorLog
    {
        public string Message { get ; set; }
        public string StackTrace { get; set; }
        public string Source { get; set; }
        public string Type { get; set; }
        public string Timestamp { get; set; }
        public Guid TraceId { get; set; }
    }
        
}