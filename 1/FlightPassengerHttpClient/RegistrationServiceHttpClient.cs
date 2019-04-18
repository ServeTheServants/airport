using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace FlightPassengerHttpClient
{
    enum RegistrationServiceAnswer
    {
        Registered = 1,
        BaggageOverweight,
        TicketNotFound
    }
    class RegistrationServiceHttpClient
    {
        private HttpClient Client { get; set; }

        public RegistrationServiceHttpClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri("http://localhost:7009/");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Client = httpClient;
        }
        public RegistrationServiceAnswer Register(FlightPassenger flightPassenger)
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(flightPassenger), Encoding.UTF8, "application/json");
            HttpResponseMessage response = Client.PostAsync("CheckIn/PostPassenger", stringContent).Result;
            if (response.StatusCode == HttpStatusCode.NoContent)
                return RegistrationServiceAnswer.Registered;
            else if (response.StatusCode == HttpStatusCode.NotFound)
                return RegistrationServiceAnswer.TicketNotFound;
            else if (response.StatusCode == HttpStatusCode.Forbidden)
                return RegistrationServiceAnswer.BaggageOverweight;
            else
                return 0;
        }
    }
}
