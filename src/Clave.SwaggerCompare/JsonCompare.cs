using System.Collections.Generic;
using System.Linq;
using Clave.SwaggerCompare.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Clave.SwaggerCompare
{
    internal class JsonCompare
    {
        static readonly JsonDifference SameResponse = new JsonDifference();

        public static JsonDifference ResponseDiffers(
            string json1,
            string json2,
            IReadOnlyCollection<string> disregardJsonResponseProperties)
        {
            var cleanJson1 = RemoveProperties(json1, disregardJsonResponseProperties);
            var cleanJson2 = RemoveProperties(json2, disregardJsonResponseProperties);
            var isSame = cleanJson1 == cleanJson2;
            return isSame
                ? SameResponse
                : new JsonDifference
                {
                    IsDifferent = true,
                    Json1 = cleanJson1,
                    Json2 = cleanJson2
                };
        }

        static string RemoveProperties(string json, IReadOnlyCollection<string> disregardJsonResponseProperties)
        {
            if (!disregardJsonResponseProperties.Any())
            {
                return json;
            }

            try
            {
                var token = JToken.Parse(json);
                foreach (var disregardJsonProperty in disregardJsonResponseProperties)
                {
                    switch (token)
                    {
                        case JArray jArray:
                            token = RemoveFromContainer(jArray, disregardJsonProperty);
                            break;
                        case JObject jObject:
                            token = RemoveFromContainer(jObject, disregardJsonProperty);
                            break;
                    }
                }
                return JsonConvert.SerializeObject(token);
            }
            catch (JsonReaderException)
            {
                return json;
            }
        }
        
        static JToken RemoveFromContainer(JContainer jContainer, string disregardJsonProperty)
        {
            if (jContainer is JObject jObject)
            {
                jObject.Remove(disregardJsonProperty);
            }

            foreach (var jProperty in jContainer.Descendants().OfType<JProperty>().Where(x => x.Name == disregardJsonProperty).ToList())
            {
                jProperty.Remove();
            }

            return JToken.Parse(jContainer.ToString());
        }
    }
}