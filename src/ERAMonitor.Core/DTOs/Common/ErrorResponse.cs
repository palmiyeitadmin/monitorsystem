namespace ERAMonitor.Core.DTOs.Common;

public class ErrorResponse
{
    public string Code { get; set; } = "ERROR";
    public string Message { get; set; } = string.Empty;
    public string? TraceId { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
    public string? StackTrace { get; set; }
}
