using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static Clave.SwaggerCompare.Logger;

namespace Clave.SwaggerCompare
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            var fileName = args.FirstOrDefault() ?? "config.json";
            if (!File.Exists(fileName))
            {
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
            LogInfo($"Response differences will be logged to {FileService.TestResultFolder()}");

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
                var swaggerUrls = await EndpointService.GetAllEndpoints(client1, testRun);

                LogInfo($"{Environment.NewLine}Starting test run #{testRunNumber++} with {swaggerUrls.Count} matching API {"call".Pluralize(swaggerUrls.Count)} planned.");
                foreach (var path in swaggerUrls)
                {
                    var client1Response = await HttpService.GetHttpResponse(path, client1, path.Url);
                    var client2Response = await HttpService.GetHttpResponse(path, client2, path.Url);

                    var jsonDifference = JsonCompare.ResponseDiffers(client1Response.jsonContent, client2Response.jsonContent, path.DisregardJsonResponseProperties);
                    var method = path.Method.Method.PadLeft(5);
                    var extraDescription = string.IsNullOrEmpty(path.FileName) ? string.Empty : $" |> {path.FileName}";
                    if (!client1Response.isSuccess)
                    {
                        LogError($"{method} | {path.Url} failed. Client1: {client1Response.statusCode}, Client2: {client2Response.statusCode}{extraDescription}");
                    }
                    else if (jsonDifference.IsDifferent)
                    {
                        await FileService.LogResponseDifference(path.Url, jsonDifference, path);
                        LogResponseDiff($"{method} | {path.Url} response differs.{extraDescription}");
                    }
                    else
                    {
                        LogSuccess($"{method} | {path.Url}{extraDescription}");
                    }
                }
            }
        }
    }
}
