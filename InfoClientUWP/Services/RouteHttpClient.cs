using System;
using System.Net.Http;

namespace InfoClientUWP.Services
{
    public static class RouteHttpClient
    {
        public static HttpClient GetRequest()
        {
            HttpClient client = new HttpClient { BaseAddress = new Uri("http://localhost:50190/") };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }
    }
}
