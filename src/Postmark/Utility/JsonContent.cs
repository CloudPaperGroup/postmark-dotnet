using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace PostmarkDotNet.Utility
{
    /// <summary>
    /// Creates a json content body suitable for posting to the API.
    /// </summary>
    internal class JsonContent<T> : StringContent
    {
        internal JsonContent(T content, JsonSerializerOptions options) :
            base(JsonSerializer.Serialize(content, options),
            Encoding.UTF8, "application/json")
        {
        }
    }
}