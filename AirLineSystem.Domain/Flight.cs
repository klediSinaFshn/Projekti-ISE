using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace AirLineSystem.Domain.Models
{
    public class Flight
    {
        //Class Properties
        public string FlightNumber {  get; set; }
        public string DepartureAirport {  get; set; }
        public string DestinationAirport { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int Capacity { get; set; }
        public int RemainingSeats { get; set; }
        public decimal TicketPrice { get; set; }
        public List<Ticket> Tickets { get; set; }
        public string FlightDuration { get; set; }

        //Constructor
        public Flight(string flightNumber,
                      string departureAirport,
                      string destinationAirport,
                      DateTime departureTime,
                      DateTime arrivalTime,
                      int capacity,
                      decimal ticketPrice,
                      string flightDuration)
        { 
            FlightNumber = flightNumber;
            DepartureAirport = departureAirport;
            DestinationAirport = destinationAirport;
            DepartureTime = departureTime;
            ArrivalTime = arrivalTime;
            Capacity = 280;    
            RemainingSeats = capacity;
            TicketPrice = ticketPrice;
            FlightDuration = flightDuration;
            Tickets = new List<Ticket>();
        }

        public void GetFlightDetails() {
         
        }

        public int CheckAvailableSeats() {
            return RemainingSeats;
        }

        public void BookSeat(Ticket ticket) {
            if (RemainingSeats > 0)
            {
                Tickets.Add(ticket);
                RemainingSeats--;
                ticket.Passenger.Tickets.Add(ticket);
            }
            else {
                Console.WriteLine("No available seats");
            }
        }

    }
}
