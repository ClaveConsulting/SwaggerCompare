using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Clave.SwaggerCompare.Models;
using static Clave.SwaggerCompare.Logger;

namespace Clave.SwaggerCompare
{
    internal static class HttpService
    {
        public static async Task<(bool isSuccess, string jsonContent, HttpStatusCode statusCode)> GetHttpResponse(SwaggerUrlWithData path, HttpClient client1, string uri)
        {
            return path.Method == HttpMethod.Get ? await GetAsync(client1, uri) :
                path.Method == HttpMethod.Post ? await HttpService.PostAsync(client1, uri, path.Data) :
                throw new Exception("Only GET and POST supported");
        }

        static async Task<(bool isSuccess, string jsonContent, HttpStatusCode statusCode)> GetAsync(HttpClient client, string url)
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
    }
}