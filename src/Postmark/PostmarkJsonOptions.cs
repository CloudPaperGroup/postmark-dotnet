using PostmarkDotNet.Converters;
using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace PostmarkDotNet
{
    /// <summary>
    /// The JSON serializer options that every Postmark client uses to read responses and write request bodies.
    /// </summary>
    /// <remarks>
    /// The defaults set <see cref="DefaultJsonTypeInfoResolver"/> explicitly, so the client still works when the host
    /// app turns reflection-based serialization off by default (<c>JsonSerializerIsReflectionEnabledByDefault=false</c>).
    /// These settings exist only in the CloudPaperGroup fork, not in the official Postmark package.
    /// </remarks>
    public static class PostmarkJsonOptions
    {
        private static JsonSerializerOptions _response = ReadOnly(CreateDefaultResponse());
        private static JsonSerializerOptions _request = ReadOnly(CreateDefaultRequest());

        /// <summary>
        /// The serializer options used to read every API response.
        /// </summary>
        /// <remarks>
        /// The default is read-only: to change it, assign new options, starting from <see cref="CreateDefaultResponse"/>
        /// to keep the converters the API's date and string formats need. A source-generated resolver must cover
        /// <see cref="PostmarkResponse"/> and every response type the client reads. Setting <c>null</c> restores the
        /// default. Do not share the instance you assign with other code: the first call makes it read-only.
        /// </remarks>
        /// <exception cref="ArgumentException">The options cannot read <see cref="PostmarkResponse"/>: they have no
        /// <see cref="JsonSerializerOptions.TypeInfoResolver"/> while reflection-based serialization is off, or their
        /// resolver does not cover the type (for example <see cref="JsonSerializerOptions.Default"/> with reflection off).
        /// </exception>
        public static JsonSerializerOptions Response
        {
            get => _response;
            set => _response = value == null
                ? ReadOnly(CreateDefaultResponse())
                : RequireResolverFor(typeof(PostmarkResponse), value, nameof(Response));
        }

        /// <summary>
        /// The serializer options used to write every request body.
        /// </summary>
        /// <remarks>
        /// The default is read-only. Start from <see cref="CreateDefaultRequest"/> to keep the string escaping the API
        /// expects. A source-generated resolver must cover <see cref="PostmarkMessage"/> and every request type the
        /// client writes, including the types of your template models: a type it does not cover makes the send throw
        /// <see cref="NotSupportedException"/>. Setting <c>null</c> restores the default. Do not share the instance you
        /// assign with other code.
        /// </remarks>
        /// <exception cref="ArgumentException">The options cannot write <see cref="PostmarkMessage"/>: they have no
        /// <see cref="JsonSerializerOptions.TypeInfoResolver"/> while reflection-based serialization is off, or their
        /// resolver does not cover the type.</exception>
        public static JsonSerializerOptions Request
        {
            get => _request;
            set => _request = value == null
                ? ReadOnly(CreateDefaultRequest())
                : RequireResolverFor(typeof(PostmarkMessage), value, nameof(Request));
        }

        /// <summary>
        /// Creates a new copy of the default <see cref="Response"/> options.
        /// </summary>
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
        /// Creates a new copy of the default <see cref="Request"/> options.
        /// </summary>
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

        private static JsonSerializerOptions RequireResolverFor(Type postmarkType, JsonSerializerOptions options, string propertyName)
        {
            var resolver = options.TypeInfoResolver;
            var canResolve = resolver == null
                ? JsonSerializer.IsReflectionEnabledByDefault
                : resolver.GetTypeInfo(postmarkType, options) != null;
            if (!canResolve)
            {
                throw new ArgumentException(
                    propertyName + " cannot handle " + postmarkType.Name + ". Use a TypeInfoResolver that covers the Postmark types, "
                    + "for example by starting from CreateDefault" + propertyName + "().",
                    "value");
            }
            return options;
        }
    }
}
