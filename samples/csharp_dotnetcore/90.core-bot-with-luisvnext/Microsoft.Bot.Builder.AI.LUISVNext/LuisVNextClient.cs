﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Bot.Builder.AI.LuisVNext
{
    internal class LuisVNextClient
    {
        /// <summary>
        /// The value type for a LUIS trace activity.
        /// </summary>
        private const string LuisVNextTraceType = "https://www.luis.ai/schemas/trace";

        [JsonIgnore]
        internal HttpClient HttpClient { get; set; }

        [JsonProperty("luisVNextOptions")]
        private LuisVNextOptions Options { get; set; }
        internal LuisVNextClient(LuisVNextOptions options, HttpClientHandler httpClientHandler)
        {
            Options = options;
            var clientHandler = httpClientHandler == default ? new HttpClientHandler() : httpClientHandler;

            HttpClient = new HttpClient(clientHandler, false)
            {
                Timeout = TimeSpan.FromMilliseconds(3000),
            };
        }

        internal async Task<JObject> Predict(string utterance, CancellationToken cancellationToken)
        {
            var uri = BuildUri(Options);
            var content = BuildRequestBody(utterance, Options);

            var request = new HttpRequestMessage(HttpMethod.Post, uri.Uri);
            var stringContent = new StringContent(content.ToString(), Encoding.UTF8, "application/json");
            request.Content = stringContent;
            request.Headers.Add("Ocp-Apim-Subscription-Key", Options.EndpointKey);

            var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return (JObject)JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        private UriBuilder BuildUri(LuisVNextOptions options)
        {
            var path = new StringBuilder(options.Endpoint);
            path.Append("/language/:analyze-conversations");

            var uri = new UriBuilder(path.ToString());
            var query = HttpUtility.ParseQueryString(uri.Query);

            query["projectName"] = options.ApplicationId;
            query["deploymentName"] = options.Slot;
            query["api-version"] = options.ApiVersion;

            uri.Query = query.ToString();
            return uri;
        }
        private static JObject BuildRequestBody(string utterance, LuisVNextOptions options)
        {
            var jsonBody = new JObject();
            jsonBody.Add("query", utterance);
            if (options.Verbose != null)
            {
                jsonBody.Add("verbose", options.Verbose);
            }
            if (options.Language != null)
            {
                jsonBody.Add("language", options.Language);
            }
            if (options.IsLoggingEnabled != null)
            {
                jsonBody.Add("isLoggingEnabled", options.IsLoggingEnabled);
            }
            return jsonBody;
        }
    }
}
