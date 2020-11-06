using System;
using System.Collections.Generic;

namespace Clave.SwaggerCompare
{
    public class LocalConfig
    {
        public string[] PossibleSwaggerUrls;
        public ClientConfig Client1 { get; set; }
        public ClientConfig Client2 { get; set; }
        public IReadOnlyCollection<TestRun> TestRuns { get; set; }
    }

    public class ClientConfig
    {
        public string Url { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    }

    public class TestRun
    {
        public IReadOnlyCollection<string> ExcludeEndpoints { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<string> TreatParametersAsRequired { get; set; } = Array.Empty<string>();
        public Dictionary<string, string> UrlParameterTestValues { get; set; }
        public IReadOnlyCollection<IncludeEndpoint> IncludeEndpoints { get; set; } = Array.Empty<IncludeEndpoint>();
    }

    public class IncludeEndpoint
    {
        public string Endpoint { get; set; }
        public string Method { get; set; }
        public string DataFolder { get; set; }
        public IReadOnlyCollection<string> DisregardJsonResponseProperties { get; set; } = Array.Empty<string>();
    }
}