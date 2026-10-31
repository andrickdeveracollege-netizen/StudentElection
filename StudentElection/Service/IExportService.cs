namespace StudentElection.Service;

public interface IExportService
{
    Task<byte[]> ExportExcelAsync();
}