namespace ERAMonitor.Core.Interfaces.Jobs;

public interface IReportGeneratorJob
{
    Task GenerateReport(Guid executionId);
}
