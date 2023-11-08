using System;

namespace Domain.Models.Results
{
    public class ProblemDetailsModel
    {
        public string ContentType { get; set; }
        public int StatusCode { get; set; }
        public string Type { get; set; }                
        public Guid TraceId { get; set; }
    }
}