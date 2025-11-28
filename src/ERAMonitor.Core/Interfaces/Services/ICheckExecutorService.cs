using ERAMonitor.Core.Entities;

namespace ERAMonitor.Core.Interfaces.Services;

public interface ICheckExecutorService
{
    Task<CheckResult> ExecuteCheckAsync(Check check);
    Task<CheckResult> ExecuteHttpCheckAsync(Check check);
    Task<CheckResult> ExecuteTcpCheckAsync(Check check);
    Task<CheckResult> ExecutePingCheckAsync(Check check);
    Task<CheckResult> ExecuteDnsCheckAsync(Check check);
}
