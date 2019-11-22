using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Clave.SwaggerCompare
{
    public class ConfigService
    {
        private const string DefaultConfigFileName = "config.json";

        public static async Task<LocalConfig> Read(string fileName)
        {
            return JsonConvert.DeserializeObject<LocalConfig>(await File.ReadAllTextAsync(GetPathTo(fileName)));
        }

        public static async Task CreateDefaultIfNotExists(Action<string> log)
        {
            var pathToDefaultConfig = GetPathTo(DefaultConfigFileName);
            if(File.Exists(pathToDefaultConfig))
                return;

            await File.WriteAllTextAsync(pathToDefaultConfig, DefaultConfig());
            log($"Created default configuration file '{DefaultConfigFileName}' at path '{pathToDefaultConfig}'");
        }

        private static string GetPathTo(string fileName) => Path.Combine(Directory.GetCurrentDirectory(), fileName);

        private static string DefaultConfig() =>
            JsonConvert.SerializeObject(new LocalConfig
            {
                Client1 = new ClientConfig
                {
                    Url = string.Empty,
                    Headers = new Dictionary<string, string>(),
                },
                Client2 = new ClientConfig
                {
                    Url = string.Empty,
                    Headers = new Dictionary<string, string>(),
                },
                TestRuns = new[]
                {
                    new TestRun
                    {
                        ReplaceValues = new Dictionary<string, string>
                        {
                            { "param1", "value1" }
                        }
                    }
                }
            }, Formatting.Indented);

        public static bool ValidateConfig(LocalConfig config, Action<string> logError)
        {
            if (string.IsNullOrEmpty(config.Client1?.Url) || string.IsNullOrEmpty(config.Client2?.Url))
            {
                logError($"Both {nameof(config.Client1.Url)} and {nameof(config.Client2.Url)} must be set.");
                return false;
            }

            if (!Uri.IsWellFormedUriString(config.Client1.Url, UriKind.Absolute) ||
                !Uri.IsWellFormedUriString(config.Client2.Url, UriKind.Absolute))
            {
                logError("Urls must be valid.");
                return false;
            }

            return true;
        }
    }
}