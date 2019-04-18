using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using FlightPassengerHttpClient;
using Bogus;
using System;

namespace FlightPassengerApi
{
    public class FlightPassengerGeneratorHostedService : BackgroundService
    { 
        //private System.Timers.Timer _timer = new System.Timers.Timer();
        private readonly DataBase _db;
        private readonly Random random = new Random();
        private static readonly object obj = new object();
        public FlightPassengerGeneratorHostedService(DataBase db)
        {
            _db = db;
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
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
            /*
            _timer.Elapsed += delegate {
                int rnd;
                lock (obj)
                    rnd = random.Next(1, 5);
                _timer.Interval = rnd;
                var flightPassenger = generator.Generate();
                flightPassenger.Start();
            };
            _timer.Start();
            */
            
            while (!stoppingToken.IsCancellationRequested)
            {
                _ = Task.Run(() =>
                {
                    var flightPassenger = generator.Generate();
                    _db.flightPassengers.Add(flightPassenger);
                    flightPassenger.Start();
                });
                int rnd;
                lock (obj)
                    rnd = random.Next(1000, 5000);
                await Task.Delay(rnd);
            }
            
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            //_timer.Stop();
            return base.StopAsync(cancellationToken);
        }
        public override void Dispose()
        {
            //_timer?.Dispose();
            base.Dispose();
        }
    }
}
