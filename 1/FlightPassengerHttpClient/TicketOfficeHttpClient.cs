using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace FlightPassengerHttpClient
{
    class TicketOfficeHttpClient
    {
        private HttpClient Client { get; set; }

        public TicketOfficeHttpClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri("http://localhost:7016/");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Client = httpClient;
        }
        public string BuyTicket(FlightPassenger flightPassenger, Flight flight)
        {
            HttpResponseMessage response = Client.GetAsync("buy/" + JsonConvert.SerializeObject(flightPassenger) + "/" + JsonConvert.SerializeObject(flight)).Result;
            if (response.IsSuccessStatusCode)
            {
                HttpContent responseContent = response.Content;
                var json = responseContent.ReadAsStringAsync().Result;
                return json;
            }
            else
                return null;
        }
    }
}
