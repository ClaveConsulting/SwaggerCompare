using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Clave.SwaggerCompare
{
    public static class SwaggerClient
    {
        private static SwaggerDocObject _swaggerResponse;
        private static readonly string[] PossibleSwaggerUrls =
            {"api/swagger/docs/v1", "swagger/docs/v1", "swagger/v1/swagger.json", "api/swagger/v1/swagger.json"};

        public static async Task<IReadOnlyCollection<SwaggerUrl>> ReadSwagger(HttpClient client, TestRun testRun)
        {
            var swaggerDocObject = await Get(client);
            if (swaggerDocObject == null) return null;
            return Map(swaggerDocObject, testRun);
        }

        private static async Task<SwaggerDocObject> Get(HttpClient client)
        {
            if (_swaggerResponse != null) return _swaggerResponse;

            foreach (var possibleSwaggerUrl in PossibleSwaggerUrls)
            {
                var swaggerResponse = await client.GetAsync(possibleSwaggerUrl);
                if (swaggerResponse.IsSuccessStatusCode)
                {
                    var swaggerDocObject = JsonConvert.DeserializeObject<SwaggerDocObject>(await swaggerResponse.Content.ReadAsStringAsync());
                    _swaggerResponse = swaggerDocObject;
                    return swaggerDocObject;
                }
            }

            return null;
        }

        private static IReadOnlyCollection<SwaggerUrl> Map(SwaggerDocObject swaggerDocObject, TestRun testRun)
        {
            return swaggerDocObject.paths.Where(x => x.Value.get != null).Select(x => new SwaggerUrl
            {
                Url = MapUrl(x, swaggerDocObject.basePath, testRun)
            }).ToArray();
        }

        private static string MapUrl(KeyValuePair<string, SwaggerEndpoint> keyValuePair, string basePath, TestRun testRun)
        {
            var initial = string.Concat(basePath ?? string.Empty, keyValuePair.Key);
            var queryParameters = keyValuePair.Value.get.parameters?.Where(x => testRun.TreatParametersAsRequired.Contains(x.name)
                                        || (x.required && x._in == "query")).ToArray() ?? Array.Empty<Parameter>();
            return string.Concat(
                initial,
                queryParameters.Any() ? "?" : string.Empty,
                string.Join("&", queryParameters.Select(x => $"{x.name}={{{x.name}}}")));
        }
    }

    public class SwaggerUrl
    {
        public string Url { get; set; }
    }
}