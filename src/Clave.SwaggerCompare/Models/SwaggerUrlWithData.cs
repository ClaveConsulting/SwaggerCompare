using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Clave.SwaggerCompare.Models
{
    internal class SwaggerUrlWithData
    {
        public string Url { get; set; }
        public HttpMethod Method { get; set; }
        public string Data { get; set; }
        public string FileNameFullPath { get; set; }
        public IReadOnlyCollection<string> DisregardJsonResponseProperties { get; set; } = Array.Empty<string>();
        public string FileName { get; set; }
    }
}