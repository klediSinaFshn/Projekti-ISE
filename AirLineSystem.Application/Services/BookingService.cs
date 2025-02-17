using AirLineSystem.Domain.Models;
using AirLineSystem.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using Stripe;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AirLineSystem.Services.Services

{
    public class BookingService : IBookingService
    {

        public delegate void PaymentConfirmedEventHandler(object sender, PaymentConfirmedEventArgs e);
        public delegate void CancelBookingEventHandler(object sender, CancelBookingEventArgs e);
        
        public event PaymentConfirmedEventHandler OnPaymentConfirmed;
        public event CancelBookingEventHandler OnCancelBooking;

        private List<Flight> _flights;
        private PaymentService _paymentService;
        private EmailService _emailService;
        private string _email;
        private  NpgsqlConnection _dbConnection;
        private FlightBookingAmadeus _flightBookingAmadeus;
        private string _paymentID;

        public string Arrival { get; set; }
        public string Departure { get; set; }
        public string DepartureDate { get; set; }

        public BookingService()
        {
            _flights = new List<Flight>();
            _paymentService = new PaymentService();
            _dbConnection = null;
            _emailService = new EmailService();
            _email = null;
            _paymentID = null;
            _flightBookingAmadeus = new FlightBookingAmadeus();
        }

        public void InitializeEventHandlers()
        {
            this.OnPaymentConfirmed -= async (sender, e) =>
            {
                await _emailService.SendEmail(e.Email, e.Type, e.PaymentID);
            };
            this.OnPaymentConfirmed -= async (sender, e) =>
            {
                await _emailService.SendEmail(e.Email, e.Type, e.PaymentID);
            };
            this.OnPaymentConfirmed -= async (sender, e) =>
            {
                await _emailService.SendEmail(e.Email, e.Type, e.PaymentID);
            };

            this.OnPaymentConfirmed += async (sender, e) =>
            {
                await _emailService.SendEmail(e.Email, e.Type, e.PaymentID);
            };
            this.OnCancelBooking += async (sender, e) =>
            {
                await _emailService.SendEmail(e.Email, e.Type,e.PaymentID);
            };
        }

        public async Task InitializeAsync()
        {
            var dbService = new AirLineSystem.Domain.Models.DbConnection();
            _dbConnection = await dbService.DbConnectionInit(); 
        }

        public static async Task<BookingService> CreateInstanceAsync()
        {
            var instance = new BookingService();
            await instance.InitializeAsync(); 
            return instance;
        }


        public async Task AddFlight(Flight flight)
        {
            _flights.Add(flight);
        }

        public async Task<string> BookFlight(decimal amount, string passenger, string ticket_number, string email, string name, string seatNumber, string flightID, string phone_number, int index)
        {
            
            if (_dbConnection == null)
            {
             
                await InitializeAsync();
            }
         
            if (_dbConnection.State != ConnectionState.Open)
            {
                await _dbConnection.OpenAsync();
            }
            _email = email;

            var book = new BookFlights(_dbConnection);
           
           
            try
            {
                _emailService.emailAdd = email;
                                
                var payment_ID_response = await _paymentService.CreateOrderAsync((amount + amount * (20/100)));

                string paymentId = payment_ID_response.Substring(0, payment_ID_response.IndexOf("_secret"));

                string[] nameParts = name.Split(' ');

               var bookingID =  await   _flightBookingAmadeus.CreateBookingAsync(index, 1, name, name, email, phone_number);

                _paymentID = paymentId;

                await book.BookFlightAsync(amount, passenger, paymentId, ticket_number, email, name, flightID, phone_number);
             
                return payment_ID_response;
                    
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
                return "Something Went Wrong";
            }
                                
        }

        public async Task<string> ConfirmPayment(string paymentID, string email)
        {
            if (_dbConnection == null)
            {
                await InitializeAsync();
            }

            if (_dbConnection.State != ConnectionState.Open)
            {
                await _dbConnection.OpenAsync();
            }

            var book = new BookFlights(_dbConnection);
            try
            {
                string confirmationResponse = await book.ConfirmBooking(paymentID, _paymentID);

                
                OnPaymentConfirmed?.Invoke(this, new PaymentConfirmedEventArgs(email, "booking", _paymentID));

                return confirmationResponse;
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }

        public async Task<string> CancelBooking(string paymentID,string bookingID, string email)
        {
            try {
                if (_dbConnection == null)
                {

                    await InitializeAsync();
                }

                if (_dbConnection.State != ConnectionState.Open)
                {
                    await _dbConnection.OpenAsync();
                }

                var book = new BookFlights(_dbConnection);                
                var databaseResponse = await book.RefundPaymentAsync(paymentID, bookingID, email);
               var  paymentResponse = await _paymentService.RefundPaymentAsync(paymentID);


                
                OnCancelBooking?.Invoke(this, new CancelBookingEventArgs(email,"refund", paymentID));

                Console.WriteLine(paymentID);
                Console.WriteLine(databaseResponse);

                return databaseResponse;

            }
            catch(Exception ex) {
                return $"Something Went Wrong {ex.Message}";
            }
        }

        //public async Task<Flight> GetFlight(string flightNumber)
        //{
        //    return 
        //}

    
        public async Task<List<Flight>> SearchFlights(string departure, string destination, DateTime date)
        {

            string formattedDate = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            FlightSearch flightSearch = new FlightSearch();
        

           
            string flightOffersResponse = await flightSearch.GetFlightOffersAsync(departure, destination, formattedDate, 1);


            //Amadeus 

            //var bookAmadeus = new FlightBookingAmadeus();


            _flightBookingAmadeus.SetFlightDetails(destination, departure, formattedDate);

            

      
            var flightOffersJson = JsonConvert.DeserializeObject<JObject>(flightOffersResponse);
            
           
            var flightOffersArray = flightOffersJson["flights"] as JArray;

            //Console.WriteLine(flightOffersArray)
            List<Flight> flightOffers = new List<Flight>();

            foreach (var offer in flightOffersArray)
            {
                
                var flight = new Flight(
                    flightID: offer["airline"]?.ToString() + offer["flightNumber"]?.ToString(),
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
