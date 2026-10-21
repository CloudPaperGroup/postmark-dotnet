using System.Text.Json;

namespace PostmarkDotNet.Converters
{
    internal static class JsonExtensions
    {
        /// <summary>
        /// Attempt to deserialize a string to the specified type.
        /// </summary>
        /// <typeparam name="T">The type that we should attempt to deserialize to.</typeparam>
        /// <param name="json">The source JSON to attempt to deserialize.</param>
        /// <param name="options">The serializer options to read with.</param>
        /// <param name="result">The object that is created, if successful.</param>
        /// <returns>True if deserialization is successful, false otherwise.</returns>
        internal static bool TryDeserializeObject<T>(string json, JsonSerializerOptions options, out T result)
        {
            var retval = false;
            result = default(T);
            try
            {
                result = JsonSerializer.Deserialize<T>(json, options);
                retval = true;
            }
            catch
            {
                //swallow this exception;   
            }
            return retval;
        }

    }
}
