namespace Clave.SwaggerCompare.Models
{
    internal class JsonDifference
    {
        public bool IsDifferent { get; set; }
        public string Json1 { get; set; } = string.Empty;
        public string Json2 { get; set; } = string.Empty;
    }
}