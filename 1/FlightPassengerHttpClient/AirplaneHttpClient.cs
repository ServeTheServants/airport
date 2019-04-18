using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace FlightPassengerHttpClient
{
    class AirplaneHttpClient
    {
        private HttpClient Client { get; set; }

        public AirplaneHttpClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri("http://localhost:7014/");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Client = httpClient;
        }
        public bool EnterTheAirplane(FlightPassenger flightPassenger)
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(flightPassenger), Encoding.UTF8, "application/json");
            HttpResponseMessage response = Client.PostAsync("planes/add_passenger/" + flightPassenger.Ticket.fID, stringContent).Result;
            if (response.IsSuccessStatusCode)
                return true;
            else
                return false;
        }
    }
}
