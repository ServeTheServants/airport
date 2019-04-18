using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace FlightPassengerHttpClient
{
    class PassengerStorageHttpClient
    {
        private HttpClient Client { get; set; }

        public PassengerStorageHttpClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri("http://localhost:4567/");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Client = httpClient;
        }
        public bool AddToPassengerStorage(Guid id)
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(id), Encoding.UTF8, "application/json");
            HttpResponseMessage response = Client.PostAsync("passenger/" + id, stringContent).Result;
            if (response.IsSuccessStatusCode)
                return true;
            else
                return false;
        }
        public bool AddToLandPassengerStorage(Guid id)
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(id), Encoding.UTF8, "application/json");
            HttpResponseMessage response = Client.PostAsync("passenger/" + id, stringContent).Result;
            if (response.IsSuccessStatusCode)
                return true;
            else
                return false;
        }
    }
}
