using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static Clave.SwaggerCompare.Logger;

namespace Clave.SwaggerCompare
{
    public static class SwaggerClient
    {
        static SwaggerDocObject _swaggerResponse;

        static readonly string[] PossibleSwaggerUrls =
            {"api/swagger/docs/v1", "swagger/docs/v1", "swagger/v1/swagger.json", "api/swagger/v1/swagger.json"};

        public static async Task<SwaggerDocObject> ReadSwagger(HttpClient client, TestRun testRun)
        {
            return await Get(client);
        }

        static async Task<SwaggerDocObject> Get(HttpClient client)
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

            LogError($"Could not find any valid swagger JSON document at {client.BaseAddress} | {string.Join(", ", PossibleSwaggerUrls)}");
            return null;
        }
    }
}