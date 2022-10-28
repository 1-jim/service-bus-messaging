using CsvHelper;
using MessagingWorkerService.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingWorkerService.Services
{
    public class OutputFileService : IOutputFileService
    {
        private readonly OutputFileConfiguration _outputFileConfiguration;
        public OutputFileService(OutputFileConfiguration outputFileConfiguration)
        {
            _outputFileConfiguration = outputFileConfiguration;
            if (!Directory.Exists(_outputFileConfiguration.OutputDirectory)) { Directory.CreateDirectory(_outputFileConfiguration.OutputDirectory); };
            if (!Directory.Exists(_outputFileConfiguration.TempDirectory)) { Directory.CreateDirectory(_outputFileConfiguration.TempDirectory); };
        }

        public bool CreateCsvOutput(string fileContentJson, string filename)
        {
            DateTimeOffset dte = DateTime.Now;
            string fileDte = dte.ToString("yyyyMMddHHmmss");
            string fullFileName = Path.Combine(_outputFileConfiguration.OutputDirectory, filename);

            var formattedContent = JsonConvert.DeserializeObject<dynamic>(fileContentJson);
            using (var writer = new StreamWriter(fullFileName))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(formattedContent);
            }
            return true;
        }

        public string CreateTempTimestampedJsonFile(dynamic data, string filename)
        {
            string directory = _outputFileConfiguration.TempDirectory;
            string fileDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            string stampedFilename = Path.Combine(directory, $"{filename}_{fileDate}.json");
            string fileContents = JsonConvert.SerializeObject(data, Formatting.Indented);
            using (StreamWriter outputFile = new StreamWriter(stampedFilename))
            {
                outputFile.Write(fileContents);
            }
            return stampedFilename;
        }

        public string ReadTempTextFile(string filename)
        {
            string directory = _outputFileConfiguration.TempDirectory;
            try
            {
                using (var textReader = new StreamReader(Path.Combine(directory, filename), System.Text.Encoding.UTF8, true))
                {
                    string datastring = textReader.ReadToEndAsync().GetAwaiter().GetResult();
                    return datastring;
                }
            }
            catch (IOException ex)
            {
                Log.Error(ex, $"ReadTextFile error on file {filename}");
            }
            return string.Empty;
        }
    }
}
