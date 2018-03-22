using InfoClientUWP.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace InfoClientUWP.Services
{
    class RequestHandler
    {
        public async Task<IEnumerable<T>> GetDataFromAPI<T>(string table)
        {
            IEnumerable<T> result = null;

            using (var client = RouteHttpClient.GetRequest())
            {
                HttpResponseMessage response = await client.GetAsync("api/" + table);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<IEnumerable<T>>(content);
                }
                else
                {
                    throw new Exception((int)response.StatusCode + " " + response.StatusCode.ToString());
                }
            }
            return result;
        }

        public async Task PostDataToAPI(CheckpointsClient checkpointToPost)
        {
            using (var client = RouteHttpClient.GetRequest())
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(checkpointToPost), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("api/Checkpoints", content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception((int)response.StatusCode + "-" + response.StatusCode.ToString());
                }
            }
        }
    }
}
