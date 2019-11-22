using System;
using System.Collections.Generic;

namespace Clave.SwaggerCompare
{
    public class LocalConfig
    {
        public ClientConfig Client1 { get; set; }
        public ClientConfig Client2 { get; set; }
        public IReadOnlyCollection<TestRun> TestRuns { get; set; }
    }

    public class ClientConfig
    {
        public string Url { get; set; }
        public Dictionary<string, string> Headers { get; set; }
    }

    public class TestRun
    {
        public IReadOnlyCollection<string> ExcludeEndpoints { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<string> IncludeEndpoints { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<string> TreatParametersAsRequired { get; set; } = Array.Empty<string>();
        public Dictionary<string, string> ReplaceValues { get; set; }
    }

    public class Header
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}