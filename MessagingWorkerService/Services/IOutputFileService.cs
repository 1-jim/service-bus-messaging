namespace MessagingWorkerService.Services
{
    public interface IOutputFileService
    {
        string CreateTempTimestampedJsonFile(dynamic data, string filename);
        string ReadTempTextFile(string filename);
        bool CreateCsvOutput(string fileContentJson, string filename);
    }
}