using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Clave.SwaggerCompare.Models;
using static Clave.SwaggerCompare.Logger;

namespace Clave.SwaggerCompare
{
    internal static class EndpointService
    {
        public static async Task<List<SwaggerUrlWithData>> GetAllEndpoints(HttpClient client, TestRun testRun, string[] possibleSwaggerUrls)
        {
            var swaggerDocObject = await SwaggerClient.ReadSwagger(client, testRun, possibleSwaggerUrls);
            if (swaggerDocObject == null)
            {
                return null;
            }
            return Map(swaggerDocObject, testRun);
        }

        static List<SwaggerUrlWithData> Map(SwaggerDocObject swaggerDocObject, TestRun testRun)
        {
            var urls = swaggerDocObject.paths.Where(x => x.Value.get != null).Select(x => new SwaggerUrl
            {
                Url = MapUrl(x, swaggerDocObject.basePath, testRun),
                Method = HttpMethod.Get
            }).Concat(swaggerDocObject.paths.Where(x => x.Value.post != null).Select(x => new SwaggerUrl
            {
                Url = MapUrl(x, swaggerDocObject.basePath, testRun),
                Method = HttpMethod.Post
            })).ToArray();
            return ExpandUrls(urls, testRun).DistinctBy(x => new { x.Url, x.Method.Method, FileName = x.FileNameFullPath }).ToList();
        }

        static string MapUrl(KeyValuePair<string, SwaggerEndpoint> keyValuePair, string basePath, TestRun testRun)
        {
            var initial = string.Concat(basePath ?? string.Empty, keyValuePair.Key);
            var queryParameters = (keyValuePair.Value.get?.parameters ?? keyValuePair.Value.post?.parameters)?.Where(
                                      x => testRun.TreatParametersAsRequired.Contains(x.name)
                                           || (x.required && x._in == "query")).ToArray() ?? Array.Empty<Parameter>();
            return string.Concat(
                initial,
                queryParameters.Any() ? "?" : string.Empty,
                string.Join("&", queryParameters.Select(x => $"{x.name}={{{x.name}}}")));
        }

        static IEnumerable<SwaggerUrlWithData> ExpandUrls(IEnumerable<SwaggerUrl> swaggerUrls, TestRun testRun)
        {
            foreach (var swaggerUrl in swaggerUrls)
            {
                if (testRun.ExcludeEndpoints.Any(s => IsMatch(swaggerUrl, s, HttpMethod.Get)))
                {
                    continue;
                }

                foreach (var specificallyIncludedEndpoint in testRun.IncludeEndpoints)
                {
                    var httpMethod = new HttpMethod(specificallyIncludedEndpoint.Method);
                    if (IsMatch(swaggerUrl, specificallyIncludedEndpoint.Endpoint, httpMethod))
                    {
                        var files = string.IsNullOrEmpty(specificallyIncludedEndpoint.DataFolder)
                            ? Array.Empty<string>()
                            : Directory.GetFiles(specificallyIncludedEndpoint.DataFolder);
                        foreach (var file in files.Any() ? files : new[] { string.Empty })
                        {
                            yield return new SwaggerUrlWithData
                            {
                                Method = httpMethod,
                                Url = ReplaceUrlParts(swaggerUrl, testRun),
                                Data = File.Exists(file) ? File.ReadAllText(file) : httpMethod == HttpMethod.Post ? "{}" : null,
                                FileNameFullPath = file,
                                FileName = Path.GetFileName(file),
                                DisregardJsonResponseProperties = specificallyIncludedEndpoint.DisregardJsonResponseProperties
                            };
                        }
                    }
                }

                if (IsMatch(swaggerUrl, swaggerUrl.Url, HttpMethod.Get))
                {
                    yield return new SwaggerUrlWithData
                    {
                        Method = HttpMethod.Get,
                        Url = ReplaceUrlParts(swaggerUrl, testRun),
                        FileNameFullPath = string.Empty,
                        FileName = string.Empty
                    };
                }
            }
        }

        static string ReplaceUrlParts(SwaggerUrl path, TestRun testRun)
        {
            var parameters = ExtractParameters(path).ToArray();
            var unknownParameters = parameters.Where(x => !testRun.UrlParameterTestValues.ContainsKey(x)).ToArray();
            if (unknownParameters.Any())
            {
                LogWarning($"Skipping {path.Url} because {string.Join(", ", unknownParameters.Select(x => x))} {"parameter".Pluralize(unknownParameters.Length)} not defined.");
                return null;
            }

            return parameters.Aggregate(path.Url, (s, param) => s.Replace($"{{{param}}}", testRun.UrlParameterTestValues[param]));
        }

        static IEnumerable<string> ExtractParameters(SwaggerUrl path)
        {
            var regex = new Regex("{(.*?)}");
            var matches = regex.Matches(path.Url);
            foreach (Match match in matches)
            {
                yield return match.Groups[1].Value;
            }
        }

        static bool IsMatch(SwaggerUrl path, string endpointPattern, HttpMethod method) =>
            path.Method == method && Regex.IsMatch(path.Url, endpointPattern.WildCardToRegular(), RegexOptions.IgnoreCase);
        static string WildCardToRegular(this string value) => "^" + Regex.Escape(value).Replace("\\*", ".*") + "$";
    }
}