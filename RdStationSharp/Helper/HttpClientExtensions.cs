using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace System.Net.Http
{
    /// <summary>
    /// Extension methods for HttpClient to simplify making requests with JSON payloads.
    /// </summary>
    internal static class HttpClientExtensions
    {
        /// <summary>
        /// Sends a POST request with a JSON payload to the specified Uri as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T">The type of the value to be serialized.</typeparam>
        /// <param name="httpClient">The HttpClient instance on which this method is called.</param>
        /// <param name="uri">The Uri the request is sent to.</param>
        /// <param name="value">The value to be serialized and included in the request content.</param>
        /// <param name="settings">(Optional) JSON serializer settings to use when serializing the value.</param>
        /// <param name="cancellationToken">(Optional) A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The value of the TResult parameter contains the HttpResponseMessage.</returns>
        public static async Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient httpClient, string uri, T value, JsonSerializerSettings settings = null, CancellationToken cancellationToken = default)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var json = JsonConvert.SerializeObject(value, settings);

            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = new StringContent(json);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await httpClient.SendAsync(request, cancellationToken);

            //RdStationSharp.Status.Status.Current.ReportProgress(response.Content.ToString());
            return response;
        }

        /// <summary>
        /// Sends a POST request without an authentication token with a JSON payload to the specified Uri as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T">The type of the value to be serialized.</typeparam>
        /// <param name="httpClient">The HttpClient instance on which this method is called.</param>
        /// <param name="uri">The Uri the request is sent to.</param>
        /// <param name="value">The value to be serialized and included in the request content.</param>
        /// <param name="settings">(Optional) JSON serializer settings to use when serializing the value.</param>
        /// <param name="cancellationToken">(Optional) A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The value of the TResult parameter contains the HttpResponseMessage.</returns>
        public static async Task<HttpResponseMessage> PostAsJsonNoTokenAsync<T>(this HttpClient httpClient, string uri, T value, JsonSerializerSettings settings = null, CancellationToken cancellationToken = default)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var json = JsonConvert.SerializeObject(value, settings);
            var token = httpClient.DefaultRequestHeaders.Authorization;
            httpClient.DefaultRequestHeaders.Authorization = null;
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = new StringContent(json);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await httpClient.SendAsync(request, cancellationToken);

            //RdStationSharp.Status.Status.Current.ReportProgress(response.Content.ToString());
            httpClient.DefaultRequestHeaders.Authorization = token;
            return response;
        }

        // <summary>
        /// Sends a PUT request with a JSON payload to the specified Uri as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T">The type of the value to be serialized.</typeparam>
        /// <param name="httpClient">The HttpClient instance on which this method is called.</param>
        /// <param name="uri">The Uri the request is sent to.</param>
        /// <param name="value">The value to be serialized and included in the request content.</param>
        /// <param name="settings">(Optional) JSON serializer settings to use when serializing the value.</param>
        /// <param name="cancellationToken">(Optional) A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The value of the TResult parameter contains the HttpResponseMessage.</returns>
        public static async Task<HttpResponseMessage> PutAsJsonAsync<T>(this HttpClient httpClient, string uri, T value, JsonSerializerSettings settings = null, CancellationToken cancellationToken = default)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var json = JsonConvert.SerializeObject(value, settings);

            var request = new HttpRequestMessage(HttpMethod.Put, uri);
            request.Content = new StringContent(json);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await httpClient.SendAsync(request, cancellationToken);

            //RdStationSharp.Status.Status.Current.ReportProgress(response.Content.ToString());
            return response;
        }

        /// <summary>
        /// Sends a PATCH request with a JSON payload to the specified Uri as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T">The type of the value to be serialized.</typeparam>
        /// <param name="httpClient">The HttpClient instance on which this method is called.</param>
        /// <param name="uri">The Uri the request is sent to.</param>
        /// <param name="value">The value to be serialized and included in the request content.</param>
        /// <param name="settings">(Optional) JSON serializer settings to use when serializing the value.</param>
        /// <param name="cancellationToken">(Optional) A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The value of the TResult parameter contains the HttpResponseMessage.</returns>
        public static async Task<HttpResponseMessage> PatchAsJsonAsync<T>(this HttpClient httpClient, string uri, T value, JsonSerializerSettings settings = null, CancellationToken cancellationToken = default)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var json = JsonConvert.SerializeObject(value, settings);

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), uri);
            request.Content = new StringContent(json);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await httpClient.SendAsync(request, cancellationToken);

            //RdStationSharp.Status.Status.Current.ReportProgress(response.Content.ToString());
            return response;
        }

        /// <summary>
        /// Sends a GET request with a JSON payload to the specified Uri as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T">The type of the value to be serialized.</typeparam>
        /// <param name="httpClient">The HttpClient instance on which this method is called.</param>
        /// <param name="uri">The Uri the request is sent to.</param>
        /// <param name="value">The value to be serialized and included in the request content.</param>
        /// <param name="settings">(Optional) JSON serializer settings to use when serializing the value.</param>
        /// <param name="cancellationToken">(Optional) A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The value of the TResult parameter contains the HttpResponseMessage.</returns>
        public static async Task<HttpResponseMessage> GetAsJsonAsync<T>(this HttpClient httpClient, string uri, T value, JsonSerializerSettings settings = null, CancellationToken cancellationToken = default)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var json = JsonConvert.SerializeObject(value, settings);

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Content = new StringContent(json);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await httpClient.SendAsync(request, cancellationToken);

            return response;
        }

        /// <summary>
        /// Sends a GET request to the specified Uri as an asynchronous operation.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance on which this method is called.</param>
        /// <param name="uri">The Uri the request is sent to.</param>
        /// <param name="cancellationToken">(Optional) A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The value of the TResult parameter contains the HttpResponseMessage.</returns>
        public static async Task<HttpResponseMessage> GetAsJsonAsync(this HttpClient httpClient, string uri, CancellationToken cancellationToken = default)
        {

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await httpClient.SendAsync(request, cancellationToken);

            //RdStationSharp.Status.Status.Current.ReportProgress(response.Content.ToString());
            return response;
        }

        /// <summary>
        /// Sends a DELETE request to the specified Uri as an asynchronous operation.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance on which this method is called.</param>
        /// <param name="uri">The Uri the request is sent to.</param>
        /// <param name="cancellationToken">(Optional) A cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The value of the TResult parameter contains the HttpResponseMessage.</returns>
        public static async Task<HttpResponseMessage> DeleteAsJsonAsync(this HttpClient httpClient, string uri, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, uri);
            var response = await httpClient.SendAsync(request, cancellationToken);

            //RdStationSharp.Status.Status.Current.ReportProgress(response.Content.ToString());
            return response;
        }

        /// <summary>
        /// Sets the Authorization header value for the HttpClient instance using the provided token.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance on which this method is called.</param>
        /// <param name="token">The token to be used in the Authorization header.</param>
        public static void SetToken(this HttpClient httpClient, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Sets the default headers for the HttpClient instance, including the Accept header with a value of "application/json".
        /// </summary>
        /// <param name="httpClient">The HttpClient instance on which this method is called.</param>
        public static void SetDefaultHeaders(this HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
