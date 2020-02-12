using System.Net.Http;

namespace Clave.SwaggerCompare.Models
{
    internal class SwaggerUrl
    {
        public string Url { get; set; }
        public HttpMethod Method { get; set; }
    }
}