using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Clave.SwaggerCompare
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var fileName = args.FirstOrDefault() ?? "config.json";
            if (string.IsNullOrEmpty(fileName))
            {
                LogWarning("Please specify a json config file name");
                await ConfigService.CreateDefaultIfNotExists(LogInfo);
                return;
            }

            var config = await ConfigService.Read(fileName);
            if (!ConfigService.ValidateConfig(config, LogError))
            {
                return;
            }

            LogInfo($"Found {config.TestRuns.Count} {"test run".Pluralize(config.TestRuns.Count)} in config.");
            LogInfo($"Client1: {config.Client1.Url}");
            LogInfo($"Client2: {config.Client2.Url}");
            LogInfo($"Response differences will be logged to {Folder()}");

            var client1 = new HttpClient
            {
                BaseAddress = new Uri(config.Client1.Url)
            };
            var client2 = new HttpClient
            {
                BaseAddress = new Uri(config.Client2.Url)
            };
            foreach (var (key, value) in config.Client1.Headers)
            {
                client1.DefaultRequestHeaders.Add(key, value);
            }
            foreach (var (key, value) in config.Client2.Headers)
            {
                client2.DefaultRequestHeaders.Add(key, value);
            }
            var testRunNumber = 1;
            foreach (var testRun in config.TestRuns)
            {
                var swaggerUrls = await SwaggerClient.ReadSwagger(client1, testRun);

                var urls = ExpandUrls(swaggerUrls, testRun).ToList();
                LogInfo($"{Environment.NewLine}Starting test run #{testRunNumber++} with {urls.Count} matching {"URL".Pluralize(urls.Count)}.");
                foreach (var path in urls)
                {
                    var uri = ReplaceUrlParts(path, testRun);
                    if (uri == null)
                    {
                        continue;
                    }

                    var client1Response = await GetHttpResponse(path, client1, uri);
                    var client2Response = await GetHttpResponse(path, client2, uri);

                    var responseDiffers = client1Response.jsonContent != client2Response.jsonContent;
                    var method = path.Method.Method.PadLeft(5);
                    if (!client1Response.isSuccess)
                    {
                        LogError($"{method} | {uri} failed. Client1: {client1Response.statusCode}, Client2: {client2Response.statusCode}");
                    }
                    else if (responseDiffers)
                    {
                        var time = DateTime.Now;
                        Directory.CreateDirectory(Folder());
                        var requestFileName = Path.GetFileName(path.FileName);
                        await File.WriteAllTextAsync(FilePath(uri, time, "client1", requestFileName), FormattedJson(client1Response.jsonContent));
                        await File.WriteAllTextAsync(FilePath(uri, time, "client2", requestFileName), FormattedJson(client2Response.jsonContent));
                        LogResponseDiff($"{method} | {uri} response differs.");
                    }
                    else
                    {
                        LogSuccess($"{method} | {uri} success.");
                    }
                }
            }
        }

        static async Task<(bool isSuccess, string jsonContent, HttpStatusCode statusCode)> GetHttpResponse(SwaggerUrlWithData path, HttpClient client1, string uri)
        {
            return path.Method == HttpMethod.Get ? await GetAsync(client1, uri) :
                path.Method == HttpMethod.Post ? await PostAsync(client1, uri, path.Data) :
                throw new Exception("Only GET and POST supported");
        }

        static IEnumerable<SwaggerUrlWithData> ExpandUrls(IEnumerable<SwaggerUrl> swaggerUrls, TestRun testRun)
        {
            foreach (var swaggerUrl in swaggerUrls)
            {
                if (testRun.ExcludeEndpoints.Any(s => IsMatch(swaggerUrl, s, HttpMethod.Get)))
                {
                    continue;
                }

                if (IsMatch(swaggerUrl, swaggerUrl.Url, HttpMethod.Get))
                {
                    yield return new SwaggerUrlWithData
                    {
                        Method = HttpMethod.Get,
                        Url = swaggerUrl.Url
                    };
                }

                foreach (var testRunIncludeNonGetEndpoint in testRun.IncludeEndpoints)
                {
                    if (IsMatch(swaggerUrl, testRunIncludeNonGetEndpoint.Endpoint, HttpMethod.Post))
                    {
                        foreach (var file in Directory.GetFiles(testRunIncludeNonGetEndpoint.DataFolder))
                        {
                            yield return new SwaggerUrlWithData
                            {
                                Method = HttpMethod.Post,
                                Url = swaggerUrl.Url,
                                Data = File.ReadAllText(file),
                                FileName = file
                            };
                        }
                    }
                }
            }
        }

        private static bool IsMatch(SwaggerUrl path, string endpointPattern, HttpMethod method) => path.Method == method && Regex.IsMatch(path.Url, endpointPattern.WildCardToRegular());

        private static string FilePath(string url, DateTime time, string env, string fileName) => Path.Combine(Folder(),
            $"{time:hh-mm-ss-fff}-{url.Replace("/", "-").SanitizeFileNamePart()}-{env}{fileName?.Prepend("-") ?? string.Empty}.json");
        private static string Folder() => Path.Combine(Directory.GetCurrentDirectory(), "test-results");
        private static string SanitizeFileNamePart(this string origFileName) => string.Join("_", origFileName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        private static string WildCardToRegular(this string value) => "^" + Regex.Escape(value).Replace("\\*", ".*") + "$";
        private static string FormattedJson(string json)
        {
            try
            {
                return JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented);
            }
            catch (Exception )
            {
                LogWarning("Response was not valid json.");
                return json;
            }
        }

        private static string Pluralize(this string word, int count) => count == 1 ? word : $"{word}s";

        private static async Task<(bool isSuccess, string jsonContent, HttpStatusCode statusCode)> GetAsync(HttpClient client, string url)
        {
            try
            {
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, content, response.StatusCode);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                return (false, "Request failed.", HttpStatusCode.InternalServerError);
            }
        }

        static async Task<(bool isSuccess, string jsonContent, HttpStatusCode statusCode)> PostAsync(HttpClient client, string url, string body)
        {
            try
            {
                var response = await client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));
                var content = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, content, response.StatusCode);
            }
            catch (Exception e)
            {
                LogError(e.ToString());
                return (false, "Request failed.", HttpStatusCode.InternalServerError);
            }
        }

        private static string ReplaceUrlParts(SwaggerUrlWithData path, TestRun testRun)
        {
            var parameters = ExtractParameters(path).ToArray();
            var unknownParameters = parameters.Where(x => !testRun.ReplaceValues.ContainsKey(x)).ToArray();
            if (unknownParameters.Any())
            {
                LogWarning($"Skipping {path.Url} because {string.Join(", ", unknownParameters.Select(x => x))} {"parameter".Pluralize(unknownParameters.Length)} not defined.");
                return null;
            }

            return parameters.Aggregate(path.Url, (s, param) => s.Replace($"{{{param}}}", testRun.ReplaceValues[param]));
        }

        private static IEnumerable<string> ExtractParameters(SwaggerUrlWithData path)
        {
            var regex = new Regex("{(.*?)}");
            var matches = regex.Matches(path.Url);
            foreach (Match match in matches)
            {
                yield return match.Groups[1].Value;
            }
        }

        private static void LogWarning(string message) => Log(message, ConsoleColor.DarkYellow);
        private static void LogError(string message) => Log(message, ConsoleColor.DarkRed);
        private static void LogResponseDiff(string message) => Log(message, ConsoleColor.Red);
        private static void LogSuccess(string message) => Log(message, ConsoleColor.DarkGreen);
        private static void LogInfo(string message) => Log(message, ConsoleColor.Blue);

        private static void Log(string message, ConsoleColor foregroundColor)
        {
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
