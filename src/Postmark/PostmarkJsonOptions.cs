using PostmarkDotNet.Converters;
using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace PostmarkDotNet
{
    /// <summary>
    /// The default JSON serializer options of the Postmark clients, to start from when you pass your own options to a
    /// client constructor.
    /// </summary>
    /// <remarks>
    /// The defaults set <see cref="DefaultJsonTypeInfoResolver"/> explicitly, so the clients still work when the host
    /// app turns reflection-based serialization off by default (<c>JsonSerializerIsReflectionEnabledByDefault=false</c>).
    /// This class and the serializer options parameters of the client constructors exist only in the CloudPaperGroup
    /// fork, not in the official Postmark package.
    /// </remarks>
    public static class PostmarkJsonOptions
    {
        internal static readonly JsonSerializerOptions DefaultResponse = ReadOnly(CreateDefaultResponse());
        internal static readonly JsonSerializerOptions DefaultRequest = ReadOnly(CreateDefaultRequest());

        /// <summary>
        /// Creates a new copy of the default options that a client uses to read API responses.
        /// </summary>
        /// <remarks>
        /// Keep the converters: the API's date and string formats need them. A source-generated resolver must cover
        /// <see cref="PostmarkResponse"/> and every response type the client reads.
        /// </remarks>
        public static JsonSerializerOptions CreateDefaultResponse() => new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            Converters =
            {
                new UnicodeJsonStringConverter(),
                new DateTimeConverter()
            }
        };

        /// <summary>
        /// Creates a new copy of the default options that a client uses to write request bodies.
        /// </summary>
        /// <remarks>
        /// Keep the converter: it gives the string escaping the API expects. A source-generated resolver must cover
        /// <see cref="PostmarkMessage"/> and every request type the client writes, including the types of your template
        /// models: a type it does not cover makes the send throw <see cref="NotSupportedException"/>.
        /// </remarks>
        public static JsonSerializerOptions CreateDefaultRequest() => new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            WriteIndented = true,
            Converters = { new UnicodeJsonStringConverter() }
        };

        private static JsonSerializerOptions ReadOnly(JsonSerializerOptions options)
        {
            options.MakeReadOnly();
            return options;
        }
    }
}
