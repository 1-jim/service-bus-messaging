using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using Serilog;

namespace MessagingWorkerService.Services
{
    public class ConfigurationUpdateService
    {
        public string GetAppSetting(string key)
        {
            var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(dirName, "appsettings.json");

            using (TextReader reader = new StreamReader(File.OpenRead(filePath)))
            {
                string jsn = reader.ReadToEnd();
                dynamic? jsonObj = JsonConvert.DeserializeObject(jsn);
                if (jsonObj == null) throw new ApplicationException("appsettings Retch Error!");
                var sectionPath = key.Split(":")[0]; 
                if (!string.IsNullOrEmpty(sectionPath))
                {
                    var keyPath = key.Split(":")[1];
                    return jsonObj[sectionPath][keyPath];
                }
                else
                {
                    return jsonObj[sectionPath];
                }
            }
        }

        public void AddOrUpdateSetting<T>(string key, T value)
        {
            try
            {
                var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var filePath = Path.Combine(dirName, "appsettings.json");
                string json = File.ReadAllText(filePath);
                if (json == null) throw new ApplicationException("appsettings Update Error!");
                dynamic? jsonObj = JsonConvert.DeserializeObject(json);

                var sectionPath = key.Split(":")[1];
                if (!string.IsNullOrEmpty(sectionPath))
                {
                    var keyPath = key.Split(':')[1];
                    jsonObj[sectionPath][keyPath] = value;
                }
                else
                {
                    jsonObj[sectionPath] = value;
                }

                string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                try
                {
                    using StreamWriter outputFile = new StreamWriter(filePath);
                    outputFile.Write(output);
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Configuration File Save Error");
                }
            }
            catch(Exception ex)
            {
                Log.Fatal(ex, "Configuration Update Error");
            }
        }
    }
}
