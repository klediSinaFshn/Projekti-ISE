using AirLineSystem.Api.DTo;
using AirLineSystem.Domain.Models;
using AirLineSystem.Services.Interfaces;
using AirLineSystem.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Forwarding;
using System.Text.Json;


namespace AirLineSystem.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookingController : ControllerBase
    {

        private readonly ILogger<BookingController> _logger;

        private readonly IBookingService _bookingService;

        private readonly IPaymentProcess _paymentProcess;

        private string _email;

        public BookingController(ILogger<BookingController> logger, IBookingService bookingService, IPaymentProcess paymentProcess)
        {
            _logger = logger;
            _bookingService = bookingService;
            _paymentProcess = paymentProcess;
            _email = null;

            if (_bookingService is BookingService bookingServiceImplementation)
            {
                bookingServiceImplementation.InitializeEventHandlers(); 
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchFlights(string departure,
                                                       string destination, DateTime date)
        {
            
            var flights = await _bookingService.SearchFlights(departure, destination, date);
            
            return Ok(flights);
        }

        [HttpPost("book")]
        [Authorize]
        public async Task<IActionResult> BookFlight(BookFlightRequest request)
        {
            
    var errorList = new List<string>();
    var checkInput = new GeneralHelper();

    errorList = checkInput.checkInput(request);

    if (errorList.Count > 0)
    {
        string jsonErrorList = JsonSerializer.Serialize(errorList);
        return BadRequest(jsonErrorList);
    }

    try
    {
        var response = await _bookingService.BookFlight(
            request.Amount,
            request.Passenger,
            request.Ticketnumber,
            request.Email,
            request.Name,
            request.seatnumber,
            request.FlightId,
            request.PhoneNumber,
            request.Index);

        // Save the email to HttpContext for later use in ConfirmPayment
        HttpContext.Items["email"] = request.Email;

        return Ok(new { response });
    }
    catch (Exception ex)
    {
        return BadRequest(new { success = false, message = ex.Message });
    }
          
            
        }
        [HttpPost("Payment/Confirm")]
        [Authorize]
        public async Task<IActionResult> ConfirmPayment(PaymentConfirmationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.clientSecret))
                {
                    return BadRequest(new { success = false, message = "Invalid parameters" });
                }
                var payment_ID_response = request.clientSecret;

                string paymentId = payment_ID_response.Substring(0, payment_ID_response.IndexOf("_secret"));
                string email = HttpContext.Items["email"] as string;

                await _bookingService.ConfirmPayment(paymentId, email);

                return Ok(new { success = true, message = "Payment was successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpDelete("cancel")]
        [Authorize]
        public async Task<IActionResult> CancelBooking([FromQuery] string paymentID, [FromQuery] string bookingID, [FromQuery] string email)
        {
            try
            {
                
                if (string.IsNullOrWhiteSpace(paymentID) || string.IsNullOrWhiteSpace(bookingID) || string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest("Both paymentID, bookingID and email are required.");
                }

                Console.WriteLine(email);
                await _bookingService.CancelBooking(paymentID, bookingID, email);

                return Ok("Booking canceled successfully");
            }
            catch (Exception ex)
            {
             
                return BadRequest($"Error while canceling booking: {ex.Message}");
            }
        }

    }
}
