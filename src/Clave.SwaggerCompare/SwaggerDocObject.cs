using System.Collections.Generic;
using Newtonsoft.Json;

namespace Clave.SwaggerCompare
{
    public class SwaggerDocObject
    {
        public string basePath { get; set; }
        public Dictionary<string, SwaggerEndpoint> paths { get; set; }
    }

    public class SwaggerEndpoint
    {
        public Get get { get; set; }
    }

    public class Get
    {
        public Parameter[] parameters { get; set; }
    }

    public class Parameter
    {
        public string name { get; set; }
        [JsonProperty("in")]
        public string _in { get; set; }
        public string description { get; set; }
        public bool required { get; set; }
        public string type { get; set; }
        public string format { get; set; }
    }

}