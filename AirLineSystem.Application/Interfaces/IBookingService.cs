using AirLineSystem.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirLineSystem.Services.Interfaces
{
    //An interface is like a bluePrint or a guideline
    public interface IBookingService
    {
        Task AddFlight(Flight flight);
        Task<List<Flight>> SearchFlights(string departure, string destination, DateTime date);
        //Task<Flight> GetFlight(string flightNumber);
        Task<string> BookFlight(decimal amount, string passenger, string ticket_number, string email, string name, string seatNumber, string flightID, string phone_number, int index);
        Task<string> ConfirmPayment(string paymentID, string email);
        Task<string> CancelBooking(string paymentID, string bookingID,string email);
        //Task InitializeFlights();


    }
}
