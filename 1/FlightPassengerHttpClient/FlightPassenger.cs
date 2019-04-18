using System;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;

namespace FlightPassengerHttpClient
{
    public enum TypeOfFood : int
    {
        Normal,
        Vegan   
    }
    public class FlightPassenger
    {
        public FlightPassenger()
        {

        }
        public FlightPassenger(Passport passport, uint baggageWeight, TypeOfFood typeOfFood)
        {
            Passport = passport;
            BaggageWeight = baggageWeight;
            TypeOfFood = typeOfFood;
        }
        private static readonly Random random = new Random();
        private static readonly object obj = new object();
        private Fsm fsm = new Fsm();
        private static readonly BoardHttpClient boardHttpClient = new BoardHttpClient(new HttpClient());
        private static readonly TicketOfficeHttpClient ticketOfficeHttpClient = new TicketOfficeHttpClient(new HttpClient());
        private static readonly RegistrationServiceHttpClient registrationServiceHttpClient = new RegistrationServiceHttpClient(new HttpClient());
        private static readonly PassengerStorageHttpClient passengerStorageHttpClient = new PassengerStorageHttpClient(new HttpClient());
        private static readonly PassengerBusHttpClient passengerBusHttpClient = new PassengerBusHttpClient(new HttpClient());
        private static readonly AirplaneHttpClient airplaneHttpClient = new AirplaneHttpClient(new HttpClient());
        private static readonly MyServiceHttpClient myServiceHttpClient = new MyServiceHttpClient(new HttpClient());
        public Passport Passport { get; set; }
        public uint BaggageWeight { get; set; }
        public TypeOfFood TypeOfFood { get; set; }
        public Ticket Ticket { get; set; }
        public List<Flight> Flights { get; set; }
        public Guid BusId { get; set; }
        private void DeleteFlightPassengerFromDBInError(Guid fpGuid)
        {
            try
            {
                if (!myServiceHttpClient.DeleteFlightPassenger(fpGuid))
                {
                    Console.WriteLine("{0} Error Deleting from Database", Passport.Surname);
                }
                else
                {
                    Console.WriteLine("{0} Deleted from BD successfully", Passport.Surname);
                    fsm.SetState(ErrorLeave);
                    fsm.Update();
                }
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("{0} My Server Isn't Working {1}", Passport.Surname, ae.Message);
            }
        }
        private void ErrorLeave()
        {
            Console.WriteLine("{0} Error trying fly, Leaving Airport in Some Error", Passport.Surname);
        }
        public void Start()
        {
            fsm.SetState(GetFlights);
            fsm.Update();
        }
        private void GetFlights()
        {
            int rnd;
            lock (obj)
                rnd = random.Next(3000, 6000);
            Thread.Sleep(rnd);
            Console.WriteLine("{0} trying to get races", Passport.Surname);
            try
            {
                var flights = boardHttpClient.GetFlights();
                if (flights != null)
                {
                    Flights = flights;
                    fsm.SetState(BuyTicket);
                    fsm.Update();
                }
                else
                {
                    Console.WriteLine("{0} GetRaces request is not OK", Passport.Surname);
                    DeleteFlightPassengerFromDBInError(Passport.Guid);
                }
            }
            catch (JsonException je)
            {
                Console.WriteLine("{0} GetRecaes Can't Desir Json {1}", Passport.Surname, je.Message);
                DeleteFlightPassengerFromDBInError(Passport.Guid);
            }
            catch (AggregateException hre)
            {
                Console.WriteLine("{0} GetRaces Server Isn't Working {1}", Passport.Surname, hre.Message);
                DeleteFlightPassengerFromDBInError(Passport.Guid);
            }
        }
        private void BuyTicket()
        {
            int rnd;
            lock (obj)
                rnd = random.Next(3000, 6000);
            Thread.Sleep(rnd);
            Console.WriteLine("{0} trying to buy ticket", Passport.Surname);
            try
            {
                Random random = new Random();
                var flightIndex = random.Next(Flights.Count);
                var flight = Flights[flightIndex];
                var json = ticketOfficeHttpClient.BuyTicket(this, flight);
                if (json != null)
                {
                    if (json == "2") //passenger with this id has already buy a ticket
                    {
                        Console.WriteLine("{0} passenger with this id has already buy a ticket", Passport.Surname);
                        DeleteFlightPassengerFromDBInError(Passport.Guid);
                    }
                    else if (json == "3") //there are no more tickets for this flight
                    {
                        Console.WriteLine("{0} there are no more tickets for this flight", Passport.Surname);
                        DeleteFlightPassengerFromDBInError(Passport.Guid);
                    }
                    else if (json == "4") //flight registration
                    {
                        Console.WriteLine("{0} flight registration ended", Passport.Surname);
                        DeleteFlightPassengerFromDBInError(Passport.Guid);
                    }
                    else if (json == "1") //flight to this city doesn't exist or plain has already landed or there is no plain for this flight - can not buy a ticket
                    {
                        Console.WriteLine("{0} flight to this city doesn't exist or plain has already landed or there is no plain for this flight - can not buy a ticket", Passport.Surname);
                        DeleteFlightPassengerFromDBInError(Passport.Guid);
                    }
                    else
                    {
                        var ticket = JsonConvert.DeserializeObject<Ticket>(json);
                        Ticket = ticket;
                    }
                }
                else
                {
                    Console.WriteLine("{0} BuyTicket request is not success", Passport.Surname);
                    DeleteFlightPassengerFromDBInError(Passport.Guid);
                }
            }
            catch (JsonException je)
            {
                Console.WriteLine("{0} BuyTicket Can't Desir Json {1}", Passport.Surname, je.Message);
                DeleteFlightPassengerFromDBInError(Passport.Guid);
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("{0} BuyTicket Server Isn't Working {1}", Passport.Surname, ae.Message);
                DeleteFlightPassengerFromDBInError(Passport.Guid);
            }
        }
        private void WaitRegistrationStart()
        {
            /*
            System.Timers.Timer t = new System.Timers.Timer();
            t.Elapsed += delegate {
                var response = boardHttpClient.GetRegistrationStatus();
                if (response == RegistrationStatus.InProgress)
                {
                    t.Stop();
                    fsm.SetState(Registration);
                    fsm.Update();
                }
                else if (response == RegistrationStatus.IsOver)
                {
                    t.Stop();
                    //fsm.SetState(GoHome);
                    fsm.Update();
                }
            };
            t.Interval = 10000;
            t.Start();
            */
            //await Task.Delay(10000);
            int rnd;
            lock (obj)
                rnd = random.Next(3000, 6000);
            Thread.Sleep(rnd);
            Console.WriteLine("{0} Waiting Registration", Passport.Surname);
            try
            {
                /*0 Ожидается регистрация 
1 Идет регистрация 
2 Началась посадка 
3 Посадка закончилась 
4 Самолет вылетел 
5 Самолет прилетел*/
                var response = boardHttpClient.GetRegistrationStatus(Ticket.fID);
                if (response == 1)
                {
                    fsm.SetState(Registration);
                    fsm.Update();
                }
                else if (response >= 2)
                {
                    Console.WriteLine("{0} Registration is over", Passport.Surname);
                    DeleteFlightPassengerFromDBInError(Passport.Guid);
                }
                else if (response == 0)
                {
                    Console.WriteLine("{0} Registration is not started yet", Passport.Surname);
                    fsm.Update();
                }
                else if (response == -1)
                {
                    Console.WriteLine("{0} Board RegistrationStatus Request is Not Ok", Passport.Surname);
                    DeleteFlightPassengerFromDBInError(Passport.Guid);
                }
            }
            catch (JsonException je)
            {
                Console.WriteLine("{0} WaitRegistration can't desir json {1}", Passport.Surname, je.Message);
                DeleteFlightPassengerFromDBInError(Passport.Guid);
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("{0} WaitRegistration Server Isn't Working {1}", Passport.Surname, ae.Message);
                DeleteFlightPassengerFromDBInError(Passport.Guid);
            }

        }

        private void Registration()
        {
            int rnd;
            lock (obj)
                rnd = random.Next(3000, 6000);
            Thread.Sleep(rnd);
            Console.WriteLine("{0} trying to Register", Passport.Surname);
            try
            {
                var rsa = registrationServiceHttpClient.Register(this);
                if (rsa == RegistrationServiceAnswer.Registered)
                {
                    fsm.SetState(EnterThePassengerStorage);
                    fsm.Update();
                }
                else if (rsa == RegistrationServiceAnswer.TicketNotFound)
                {
                    Console.WriteLine("{0} Ticket Not Found", Passport.Surname);
                    DeleteFlightPassengerFromDBInError(Passport.Guid);
                }
                else if (rsa == RegistrationServiceAnswer.BaggageOverweight)
                {
                    Console.WriteLine("{0} Baggage OverWeight", Passport.Surname);
                    DeleteFlightPassengerFromDBInError(Passport.Guid);
                }
                else if (rsa == 0)
                {
                    Console.WriteLine("{0} Registraion Service Another Error Status Code", Passport.Surname);
                    DeleteFlightPassengerFromDBInError(Passport.Guid);
                }
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("{0} RegistrationService Server Isn't Working {1}", Passport.Surname, ae.Message);
                DeleteFlightPassengerFromDBInError(Passport.Guid);
            }
        }
        private void EnterThePassengerStorage()
        {
            int rnd;
            lock (obj)
                rnd = random.Next(3000, 6000);
            Thread.Sleep(rnd);
            Console.WriteLine("{0} trying to Add to Passenger Storage", Passport.Surname);
            try
            {
                if (passengerStorageHttpClient.AddToPassengerStorage(Passport.Guid))
                {
                    fsm.SetState(WaitBusArriveToPassengerStorage);
                    fsm.Update();
                }
                else
                {
                    Console.WriteLine("{0} Error Entering Passenger Storage", Passport.Surname);
                    DeleteFlightPassengerFromDBInError(Passport.Guid);
                }
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("{0} PassengerStorage Server Isn't Working {1}", Passport.Surname, ae.Message);
                DeleteFlightPassengerFromDBInError(Passport.Guid);
            }
        }
        private void WaitBusArriveToPassengerStorage()
        {
            Console.WriteLine("{0} Waiting Bus", Passport.Surname);
            while (fsm.ActiveState != EnterTheBus)
            {
                Thread.Sleep(1);
            }
            fsm.Update();
        }
        public void SetEnterTheBusState(Guid busId)
        {
            Console.WriteLine("{0} Bus Arrived to the Passenger storage", Passport.Surname);
            if (fsm.ActiveState == WaitBusArriveToPassengerStorage)
            {
                BusId = busId;
                fsm.SetState(EnterTheBus);
            }
            else
            {
                Console.WriteLine("{0} is not waiting for the bus, can't change state", Passport.Surname);
            }
        }
        private void EnterTheBus()
        {
            int rnd;
            lock (obj)
                rnd = random.Next(3000, 6000);
            Thread.Sleep(rnd);
            Console.WriteLine("{0} Going To The Bus", Passport.Surname);
            try
            {
                if (!passengerBusHttpClient.EnterTheBus(Passport.Guid, BusId))
                {
                    Console.WriteLine("{0} Error entering Bus", Passport.Surname);
                    DeleteFlightPassengerFromDBInError(Passport.Guid);
                }
                else
                {
                    fsm.SetState(WaitBusFinishMotionToTheAirplane);
                    fsm.Update();
                }
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("{0} Bus Server Isn't Working {1}", Passport.Surname, ae.Message);
                DeleteFlightPassengerFromDBInError(Passport.Guid);
            }
        }
        private void WaitBusFinishMotionToTheAirplane()
        {
            Console.WriteLine("{0} Waiting Bus Finish Motion To The Airplane", Passport.Surname);
            while (fsm.ActiveState != EnterTheAirplane)
            {
                Thread.Sleep(1);
            }
            fsm.Update();
        }
        public void SetEnterTheAirplaneState()
        {
            Console.WriteLine("{0} Bus finished motion to the airplane", Passport.Surname);
            if (fsm.ActiveState == WaitBusFinishMotionToTheAirplane)
            {
                fsm.SetState(EnterTheAirplane);
            }
            else
            {
                Console.WriteLine("{0} is not waiting bus motion to airplane, can't change state", Passport.Surname);
            }
        }
        private void EnterTheAirplane()
        {
            Console.WriteLine("{0} Going To The Airplane", Passport.Surname);
            int rnd;
            lock (obj)
                rnd = random.Next(3000, 6000);
            Thread.Sleep(rnd);
            try
            {
                if (!airplaneHttpClient.EnterTheAirplane(this))
                {
                    Console.WriteLine("{0} Error entering Airport", Passport.Surname);
                    DeleteFlightPassengerFromDBInError(Passport.Guid);
                }
                else
                {
                    fsm.SetState(WaitTakeoff);
                    fsm.Update();
                }
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("{0} Airplane Server Isn't Working {1}", Passport.Surname, ae.Message);
                DeleteFlightPassengerFromDBInError(Passport.Guid);
            }
        }
        private void WaitTakeoff()
        {
            Console.WriteLine("{0} Waiting Takeoff", Passport.Surname);
            //var lol = SpinWait.SpinUntil(() => fsm.ActiveState != Takeoff, Timeout.Infinite);
            while (fsm.ActiveState != Takeoff)
            {
                Thread.Sleep(1);
            }
            fsm.Update();
        }
        public void SetTakeoffState()
        {
            if (fsm.ActiveState == WaitTakeoff)
            {
                fsm.SetState(Takeoff);
            }
            else
            {
                Console.WriteLine("{0} is not waiting Take-Off, can't change state", Passport.Surname);
            }
        }
        private void Takeoff()
        {
            Console.WriteLine("{0} Trying TakeOff", Passport.Surname);
            try
            {
                if (!myServiceHttpClient.DeleteFlightPassenger(Passport.Guid))
                {
                    Console.WriteLine("{0} Error Deleting from Database", Passport.Surname);
                }
                else
                {
                    Console.WriteLine("{0} Deleted from BD successfully", Passport.Surname);
                    fsm.SetState(End);
                    fsm.Update();
                }
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("{0} My Server Isn't Working {1}", Passport.Surname, ae.Message);
            }
        }
        private void End()
        {
            Console.WriteLine("{0} Ends fly success, Thank you!", Passport.Surname);
        }
        
        public void StartFromAirplane()
        {
            Console.WriteLine("{0} Starting from Airplane, Flight GUID: {1}", Passport.Surname, Ticket.fID);
            fsm.SetState(WaitAirplaneLand);
            fsm.Update();
        }
        private void WaitAirplaneLand()
        {
            Console.WriteLine("{0} Waiting airplane land, Flight GUID: {1}", Passport.Surname, Ticket.fID);
            while (fsm.ActiveState != WaitLandBus)
            {
                Thread.Sleep(1);
            }
            fsm.Update();
        }
        public void SetWaitLandBusState()
        {
            if (fsm.ActiveState == WaitAirplaneLand)
            {
                fsm.SetState(WaitLandBus);
            }
            else
            {
                Console.WriteLine("{0} is not waiting AirplaneLand, Flight GUID: {1}, can't change state", Passport.Surname, Ticket.fID);
            }
        }
        private void WaitLandBus()
        {
            Console.WriteLine("{0} Waiting Land Bus, Flight GUID: {1}", Passport.Surname, Ticket.fID);
            while (fsm.ActiveState != EnterTheLandBus)
            {
                Thread.Sleep(1);
            }
            fsm.Update();
        }
        public void SetEnterTheLandBusState(Guid landBusId)
        {
            if (fsm.ActiveState == WaitLandBus)
            {
                BusId = landBusId;
                fsm.SetState(EnterTheLandBus);
            }
            else
            {
                Console.WriteLine("{0} is not waiting bus motion to Land Airplane, can't change state", Passport.Surname);
            }
        }
        private void EnterTheLandBus()
        {
            int rnd;
            lock (obj)
                rnd = random.Next(3000, 6000);
            Thread.Sleep(rnd);
            Console.WriteLine("{0} Going To The Land Bus, Flight GUID: {1}", Passport.Surname, Ticket.fID);
            try
            {
                if (!passengerBusHttpClient.EnterTheLandBus(Passport.Guid, BusId))
                {
                    Console.WriteLine("{0} Error entering Land Bus, Flight GUID: {1}", Passport.Surname, Ticket.fID);
                    DeleteFlightPassengerFromLandDBInError(Passport.Guid);
                }
                else
                {
                    fsm.SetState(WaitBusFinishMotionToTheLandPassengerStorage);
                    fsm.Update();
                }
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("{0} Land Bus Server Isn't Working {1}", Passport.Surname, ae.Message);
                DeleteFlightPassengerFromLandDBInError(Passport.Guid);
            }
        }
        private void WaitBusFinishMotionToTheLandPassengerStorage()
        {
            Console.WriteLine("{0} Waiting Bus Finish Motion To The Land Passenger Storage, Flight GUID: {1}", Passport.Surname);
            while (fsm.ActiveState != EnterTheLandPassengerStorage)
            {
                Thread.Sleep(1);
            }
            fsm.Update();
        }
        public void SetEnterTheLandPassengerStorageState()
        {
            if (fsm.ActiveState == WaitBusFinishMotionToTheLandPassengerStorage)
            {
                fsm.SetState(EnterTheLandPassengerStorage);
            }
            else
            {
                Console.WriteLine("{0} is not waiting bus motion to Land Passenger Storage, Flight GUID: {1}, can't change state", Passport.Surname, Ticket.fID);
            }
        }
        private void EnterTheLandPassengerStorage()
        {
            int rnd;
            lock (obj)
                rnd = random.Next(3000, 6000);
            Thread.Sleep(rnd);
            Console.WriteLine("{0} trying to Add to Land Passenger Storage, Flight GUID: {1}", Passport.Surname, Ticket.fID);
            try
            {
                if (passengerStorageHttpClient.AddToLandPassengerStorage(Passport.Guid))
                {
                    fsm.SetState(LeaveLandPassengerStorage);
                    fsm.Update();
                }
                else
                {
                    Console.WriteLine("{0} Error Entering Land Passenger Storage", Passport.Surname);
                    DeleteFlightPassengerFromLandDBInError(Passport.Guid);
                }
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("{0} Land PassengerStorage Server Isn't Working {1}", Passport.Surname, ae.Message);
                DeleteFlightPassengerFromLandDBInError(Passport.Guid);
            }
        }
        private void LeaveLandPassengerStorage()
        {
            int rnd;
            lock (obj)
                rnd = random.Next(3000, 6000);
            Thread.Sleep(rnd);
            Console.WriteLine("{0} Trying Go Home from Land Passenger Storage", Passport.Surname);
            try
            {
                if (!myServiceHttpClient.DeleteArrivedFlightPassenger(Passport.Guid))
                {
                    Console.WriteLine("{0} Error Deleting arrived fp from Database", Passport.Surname);
                }
                else
                {
                    Console.WriteLine("{0} Deleted arrived passenger from BD successfully", Passport.Surname);
                    fsm.SetState(EndArrive);
                    fsm.Update();
                }
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("{0} My Server Isn't Working {1}", Passport.Surname, ae.Message);
            }
       
        }
        private void EndArrive()
        {
            Console.WriteLine("{0} Seems like that's all yet, GOING HOME FROM AIRPORT Successfully, FLIGHT GUID: {1}", Passport.Surname, Ticket.fID);
        }
        private void DeleteFlightPassengerFromLandDBInError(Guid fpGuid)
        {
            int rnd;
            lock (obj)
                rnd = random.Next(3000, 6000);
            Thread.Sleep(rnd);
            try
            {
                if (!myServiceHttpClient.DeleteArrivedFlightPassenger(Passport.Guid))
                {
                    Console.WriteLine("{0} Error Deleting arrived fp from Database", Passport.Surname);
                }
                else
                {
                    Console.WriteLine("{0} Deleted arrived passenger from BD successfully", Passport.Surname);
                    fsm.SetState(ErrorLeaveLand);
                    fsm.Update();
                }
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("{0} My Server Isn't Working {1}", Passport.Surname, ae.Message);
            }
        }
        private void ErrorLeaveLand()
        {
            Console.WriteLine("{0} Leaving Airport With Landing Error", Passport.Surname);
        }
    }
}
