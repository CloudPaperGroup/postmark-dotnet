using PostmarkDotNet.Converters;
using PostmarkDotNet.Exceptions;
using PostmarkDotNet.Utility;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Postmark.Exceptions;

namespace PostmarkDotNet
{
    /// <summary>
    /// The core postmark client code.
    /// </summary>
    public abstract class PostmarkClientBase
    {
        private static Lazy<ISimpleHttpClient> _staticClient =
            new Lazy<ISimpleHttpClient>(()=>new SimpleHttpClient()) ;

        /// <summary>
        /// Configure a global connection factory to to process HTTP interactions.
        /// </summary>
        /// <remarks>
        /// In most cases, you should not need to modify this property, but it's useful
        /// in cases where you want to use another http client, or to mock the http processing
        /// (for tests).
        /// </remarks>
        public static Func<ISimpleHttpClient> ClientFactory {get;set;} = () => _staticClient.Value;

        private static JsonSerializerOptions _responseSerializerOptions = ReadOnly(CreateDefaultResponseSerializerOptions());
        private static JsonSerializerOptions _requestSerializerOptions = ReadOnly(CreateDefaultRequestSerializerOptions());

        /// <summary>
        /// The serializer options used to read every API response.
        /// </summary>
        /// <remarks>
        /// The default sets <see cref="DefaultJsonTypeInfoResolver"/> explicitly, so responses still read when the
        /// host app turns reflection-based serialization off by default
        /// (<c>JsonSerializerIsReflectionEnabledByDefault=false</c>). The default is read-only: to change it, assign new
        /// options, starting from <see cref="CreateDefaultResponseSerializerOptions"/> to keep the converters the API's
        /// date and string formats need. A source-generated resolver must cover <see cref="PostmarkResponse"/> and every
        /// response type the client reads. Setting <c>null</c> restores the default. Do not share the instance you assign
        /// with other code: the first call makes it read-only.
        /// </remarks>
        /// <exception cref="ArgumentException">The options have no <see cref="JsonSerializerOptions.TypeInfoResolver"/>
        /// and reflection-based serialization is off, so they could not read any response.</exception>
        public static JsonSerializerOptions ResponseSerializerOptions
        {
            get => _responseSerializerOptions;
            set => _responseSerializerOptions = value == null
                ? ReadOnly(CreateDefaultResponseSerializerOptions())
                : RequireResolver(value, nameof(ResponseSerializerOptions));
        }

        /// <summary>
        /// The serializer options used to write every request body.
        /// </summary>
        /// <remarks>
        /// The default sets <see cref="DefaultJsonTypeInfoResolver"/> explicitly, for the same reason as
        /// <see cref="ResponseSerializerOptions"/>, and is read-only. Start from
        /// <see cref="CreateDefaultRequestSerializerOptions"/> to keep the string escaping the API expects. Setting
        /// <c>null</c> restores the default. Do not share the instance you assign with other code.
        /// </remarks>
        /// <exception cref="ArgumentException">The options have no <see cref="JsonSerializerOptions.TypeInfoResolver"/>
        /// and reflection-based serialization is off, so they could not write any request.</exception>
        public static JsonSerializerOptions RequestSerializerOptions
        {
            get => _requestSerializerOptions;
            set => _requestSerializerOptions = value == null
                ? ReadOnly(CreateDefaultRequestSerializerOptions())
                : RequireResolver(value, nameof(RequestSerializerOptions));
        }

        private static JsonSerializerOptions ReadOnly(JsonSerializerOptions options)
        {
            options.MakeReadOnly();
            return options;
        }

        private static JsonSerializerOptions RequireResolver(JsonSerializerOptions options, string propertyName)
        {
            if (options.TypeInfoResolver == null && !JsonSerializer.IsReflectionEnabledByDefault)
            {
                throw new ArgumentException(
                    propertyName + " needs a TypeInfoResolver, because reflection-based serialization is disabled in this app.",
                    propertyName);
            }
            return options;
        }

        /// <summary>
        /// Creates a new copy of the default <see cref="ResponseSerializerOptions"/>.
        /// </summary>
        public static JsonSerializerOptions CreateDefaultResponseSerializerOptions() => new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            Converters =
            {
                new UnicodeJsonStringConverter(),
                new DateTimeConverter()
            }
        };

        /// <summary>
        /// Creates a new copy of the default <see cref="RequestSerializerOptions"/>.
        /// </summary>
        public static JsonSerializerOptions CreateDefaultRequestSerializerOptions() => new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            WriteIndented = true,
            Converters = { new UnicodeJsonStringConverter() }
        };

        protected static readonly string DATE_FORMAT = "yyyy-MM-dd";

        /// <summary>
        /// Default transactional message stream id.
        /// </summary>
        protected const string DefaultTransactionalStream = "outbound";

        private static readonly string _agent = "Postmark.NET 2.x (" +
              typeof(PostmarkClient).AssemblyQualifiedName + ")";

        private Uri baseUri;

        /// <summary>
        /// Provides a base implementation of core request/response interactions.
        /// </summary>
        /// <param name="apiBaseUri"></param>
        /// <param name="requestTimeoutInSeconds"></param>
        public PostmarkClientBase(string apiBaseUri = "https://api.postmarkapp.com")
        {
            baseUri = new Uri(apiBaseUri);
        }

        protected abstract string AuthHeaderName { get; }

        protected string _authToken;

        /// <summary>
        /// The core delivery method for all other API methods.
        /// </summary>
        /// <typeparam name="TRequestBody"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="apiPath"></param>
        /// <param name="verb"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        protected async Task<TResponse> ProcessRequestAsync<TRequestBody, TResponse>(
            string apiPath,
            HttpMethod verb, TRequestBody message = null) where TRequestBody : class
        {
            var client = ClientFactory();

            var request = new HttpRequestMessage(verb, baseUri + apiPath.TrimStart('/'));

            //if the message is not a string, or the message is a non-empty string,
            //set a body for this request.
            if (message != null)
            {
                var content = new JsonContent<TRequestBody>(message, RequestSerializerOptions);
                request.Content = content;
            }

            request.Headers.Add("Accept", "application/json");
            request.Headers.Add(AuthHeaderName, _authToken);
            request.Headers.Add("User-Agent", _agent);

            var result = await client.SendAsync(request);

            var body = await result.Content.ReadAsStringAsync();

            var options = ResponseSerializerOptions;

            if (JsonExtensions.TryDeserializeObject<TResponse>(body, options, out TResponse parsedResponse, out _)
                && result.StatusCode == HttpStatusCode.OK)
            {
                return parsedResponse;
            }

            if (!JsonExtensions.TryDeserializeObject<PostmarkResponse>(body, options, out PostmarkResponse error, out var parseFailure))
            {
                throw new PostmarkResponseException("The response from the API could not be parsed.", body, parseFailure);
            }

            switch ((int) result.StatusCode)
            {
                case 422:
                    error.Status = PostmarkStatus.UserError;
                    break;
                case 500:
                    error.Status = PostmarkStatus.ServerError;
                    break;
                default:
                    error.Status = PostmarkStatus.Unknown;
                    break;
            }

            throw new PostmarkValidationException(error);
        }

        /// <summary>
        /// Core implementation of HTTP interaction for "no body" requests (i.e. GET/DELETE)
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="apiPath"></param>
        /// <param name="parameters"></param>
        /// <param name="verb">The http verb to use for the request.</param>
        /// <returns></returns>
        protected async Task<TResponse> ProcessNoBodyRequestAsync<TResponse>(
            string apiPath,
            IDictionary<string, object> parameters = null,
            HttpMethod verb = null)
        {
            parameters = parameters ?? new Dictionary<string, object>();

            var query = "";
            foreach (var param in parameters)
            {
                if (param.Value != null)
                {
                    if (query != "")
                    {
                        query += "&";
                    }
                    query += String.Format("{0}={1}", WebUtility.UrlEncode(param.Key), WebUtility.UrlEncode(param.Value.ToString()));
                }
            }

            if (!String.IsNullOrWhiteSpace(query))
            {
                apiPath = apiPath + "?" + query;
            }

            return await ProcessRequestAsync<object, TResponse>(apiPath, verb ?? HttpMethod.Get);
        }

    }
}
