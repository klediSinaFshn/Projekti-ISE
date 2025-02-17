using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirLineSystem.Api.DTo
{
    public class BookFlightRequest
    {
        public decimal Amount { get; set; }
        public string Name { get; set; }
        public string FlightId { get; set; }        
        public string Ticketnumber {  get; set; }
        public string Email {  get; set; }
        public string PhoneNumber { get; set; }
        public string seatnumber { get; set; }
        public string Passenger {  get; set; }
        public int Index { get; set; }


    }
}

//public Task<string> BookFlight(decimal amount, string passenger, string ticket_number, string email, string name, string seatNumber, string flightID, string phone_number);
