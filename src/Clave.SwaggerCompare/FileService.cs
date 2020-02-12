using System;
using System.IO;
using System.Threading.Tasks;
using Clave.SwaggerCompare.Models;
using Newtonsoft.Json;
using static Clave.SwaggerCompare.Logger;

namespace Clave.SwaggerCompare
{
    internal static class FileService
    {
        public static async Task LogResponseDifference(string uri, JsonDifference jsonDifference, SwaggerUrlWithData swaggerUrl)
        {
            var time = DateTime.Now;
            Directory.CreateDirectory(TestResultFolder());
            await File.WriteAllTextAsync(FilePath(uri, time, "client1", swaggerUrl.FileName), FormattedJson(jsonDifference.Json1));
            await File.WriteAllTextAsync(FilePath(uri, time, "client2", swaggerUrl.FileName), FormattedJson(jsonDifference.Json2));
        }

        public static string TestResultFolder() => Path.Combine(Directory.GetCurrentDirectory(), "test-results");

        static string FilePath(string url, DateTime time, string env, string fileName) => Path.Combine(TestResultFolder(),
            $"{time:hh-mm-ss-fff}-{url.Replace("/", "-").SanitizeFileNamePart()}-{env}{fileName?.Prepend("-") ?? string.Empty}.json");

        static string SanitizeFileNamePart(this string origFileName) => string.Join("_", origFileName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

        static string FormattedJson(string json)
        {
            try
            {
                return JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented);
            }
            catch (Exception)
            {
                LogWarning("Response was not valid json.");
                return json;
            }
        }
    }
}