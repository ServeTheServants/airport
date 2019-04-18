using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FlightPassengerHttpClient;
using Bogus;

namespace FlightPassengerApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FlightPassengersController : ControllerBase
    {
        private readonly DataBase _db;
        public FlightPassengersController(DataBase db)
        {
            _db = db;
        }
        [HttpPost]
        public ActionResult BusArrivedToPassengerStorage([FromBody] (int flightId, Guid busId, int numberOfBusSeats) flightBus)
        {
            var busPassengers = _db.flightPassengers.Where((fp) => fp.Ticket.fID == flightBus.flightId).Take(flightBus.numberOfBusSeats).ToList();
            foreach (var bp in busPassengers)
            {
                Task.Run(() => bp.SetEnterTheBusState(flightBus.busId));
            }
            return NoContent();
        }

        [HttpPost]
        public ActionResult BusArrivedToAirplane([FromBody] Guid busId)
        {
            var busPassengers = _db.flightPassengers.FindAll((fp) => fp.BusId == busId);
            foreach (var bp in busPassengers)
            {
                Task.Run(() => bp.SetEnterTheAirplaneState());
            }
            return NoContent();
        }
        [HttpPost]
        public ActionResult Takeoff([FromBody] int flightId)
        {
            var airplanePassengers = _db.flightPassengers.FindAll((fp) => fp.Ticket.fID == flightId);
            foreach (var ap in airplanePassengers)
            {
                Task.Run(() => ap.SetTakeoffState());
            }
            return NoContent();
        }
        [HttpDelete("{id:Guid}")]
        public ActionResult Delete(Guid id)
        {
            _db.flightPassengers.RemoveAll((fp) => fp.Passport.Guid == id);
            return NoContent();
        }
        [HttpPost]
        public ActionResult<List<FlightPassenger>> GenerateFlightPassengersInAirplane([FromBody] (Flight flight, int numberOfAirplanePassnegers) flightNum)
        {
            var fakePassports = new Faker<Passport>()
                .StrictMode(true)
                .RuleFor(x => x.Guid, f => f.Random.Guid())
                .RuleFor(x => x.DateOfBirth, f => f.Person.DateOfBirth)
                .RuleFor(x => x.GivenNames, f => f.Person.FirstName)
                .RuleFor(x => x.Surname, f => f.Person.LastName)
                .RuleFor(x => x.Nationality, f => f.Address.Country())
                .RuleFor(x => x.Sex, f => f.Random.Enum<Sex>());
            var generator = new Faker<FlightPassenger>()
                .RuleFor(x => x.Passport, () => fakePassports)
                .RuleFor(x => x.BaggageWeight, f => f.Random.UInt(0, 100))
                .RuleFor(x => x.TypeOfFood, f => f.Random.Enum<TypeOfFood>());
            var airplanePassengers = new List<FlightPassenger>();
            for (int i = 0; i < flightNum.numberOfAirplanePassnegers; i++)
            {
                var flightPassenger = generator.Generate();
                Ticket ticket = new Ticket(Guid.NewGuid(), flightPassenger.Passport.Guid, flightNum.flight.reisNumber, flightNum.flight.to, flightPassenger.Passport.GivenNames, flightPassenger.Passport.Surname, flightPassenger.Passport.Sex);
                flightPassenger.Ticket = ticket;
                _db.arriveFlightPassengers.Add(flightPassenger);
                airplanePassengers.Add(flightPassenger);
            }
            foreach (var afp in airplanePassengers)
            {
                Task.Run(() =>
                {
                    afp.StartFromAirplane();
                });
            }
            return Ok(airplanePassengers);
        }
        [HttpPost]
        public ActionResult AirplaneLanded([FromBody] int flightId)
        {
            var airplanePassengers = _db.arriveFlightPassengers.FindAll((ap) => ap.Ticket.fID == flightId);
            foreach (var ap in airplanePassengers)
            {
                Task.Run(() => ap.SetWaitLandBusState());
            }
            return NoContent();
        }
        [HttpPost]
        public ActionResult BusArrivedToLandedAirplane([FromBody] (int flightId, Guid busId, int numberOfBusSeats) flightBus)
        {
            var busPassengers = _db.arriveFlightPassengers.Where((fp) => fp.Ticket.fID == flightBus.flightId).Take(flightBus.numberOfBusSeats).ToList();
            foreach (var bp in busPassengers)
            {
                Task.Run(() => bp.SetEnterTheLandBusState(flightBus.busId));
            }
            return NoContent();
        }

        [HttpPost]
        public ActionResult BusArrivedToLandedPassengerStorage([FromBody] Guid busId)
        {
            var busPassengers = _db.flightPassengers.FindAll((fp) => fp.BusId == busId);
            foreach (var bp in busPassengers)
            {
                Task.Run(() => bp.SetEnterTheLandPassengerStorageState());
            }
            return NoContent();
        }
        [HttpDelete("{id:Guid}")]
        public ActionResult DeleteArrived(Guid id)
        {
            _db.arriveFlightPassengers.RemoveAll((fp) => fp.Passport.Guid == id);
            return NoContent();
        }
    }
}