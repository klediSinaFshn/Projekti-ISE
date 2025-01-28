using AirLineSystem.Domain.Models;
using AirLineSystem.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirLineSystem.Services.Services
{
    public class BookingService : IBookingService
    {
        private List<Flight> _flights;
        public BookingService() { 
            _flights = new List<Flight>();
            //InitializeFlights();
        }

        public async Task AddFlight(Flight flight)
        {
            _flights.Add(flight);
        }

        public async Task BookFlight(Flight flight, Passenger passenger, string seatNumber)
        {
            var ticket = new Ticket(Guid.NewGuid().ToString(), passenger, flight,seatNumber);
            flight.BookSeat(ticket);
            //Assume payment handling here
            Console.WriteLine($"Ticket booked successfully for {passenger.Name}, Payment proccessed.");
        }

        public async Task CancelBooking(string ticketNumber)
        {
            foreach (var flight in _flights) 
                { var ticket = flight.Tickets.Find(t => t.TicketNumber == ticketNumber);
                if (ticket != null) {
                        flight.Tickets.Remove(ticket);
                        flight.RemainingSeats++;
                        ticket.Passenger.Tickets.Remove(ticket);
                    //Assume refund processing
                    Console.WriteLine($"Booking canceled for ticket {ticketNumber}. Refund proccessed.");
                    break;
                    }
                }
        }

        public async Task<Flight> GetFlight(string flightNumber)
        {
            return _flights.Where(f => f.FlightNumber == flightNumber).First();
        }

    
        public async Task<List<Flight>> SearchFlights(string departure, string destination, DateTime date)
        {

            string formattedDate = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            FlightSearch flightSearch = new FlightSearch();
            // Call the GetFlightOffersAsync method and await the result
           
            string flightOffersResponse = await flightSearch.GetFlightOffersAsync(departure, destination, formattedDate, 1);
          
            // Parse the response into a JObject
            var flightOffersJson = JsonConvert.DeserializeObject<JObject>(flightOffersResponse);
            
            // Retrieve the "flights" array from the JObject
            var flightOffersArray = flightOffersJson["flights"] as JArray;

            //Console.WriteLine(flightOffersArray)
            List<Flight> flightOffers = new List<Flight>();

            foreach (var offer in flightOffersArray)
            {
                // Assuming the JSON data contains these fields directly; adjust if necessary
                var flight = new Flight(
                    flightNumber: offer["flightNumber"]?.ToString(),
                    departureAirport: departure.ToUpper().ToString(),
                    destinationAirport: destination.ToUpper().ToString(),
                    departureTime: DateTime.Parse(offer["departureTime"]?.ToString()),
                    arrivalTime: DateTime.Parse(offer["arrivalTime"]?.ToString()),
                    capacity: 280,
                    ticketPrice: offer["price"] != null ? decimal.Parse(offer["price"].ToString()) : 0.0m,
                    flightDuration: offer["flightDuration"]?.ToString()
                );

                flight.RemainingSeats = offer["seatsAvailable"] != null ? int.Parse(offer["seatsAvailable"].ToString()) : 0;


                flightOffers.Add(flight);
            }

            // Return the list of flights
            return flightOffers;
        }

        //private string FormatDateTime(DateTime dateTime)
        //{
        //  return  dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        //}
    }
}
