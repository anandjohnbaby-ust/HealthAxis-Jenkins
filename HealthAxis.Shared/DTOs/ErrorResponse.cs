using System.ComponentModel.DataAnnotations;
using System.Net;

namespace HealthAxis.API.DTOs
{
    public class ErrorResponse
    {
        public int StatusCode {get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Path { get; set; } = string.Empty;
    }
}
