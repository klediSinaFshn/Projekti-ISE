using AirLineSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace AirLineSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
        }

        [HttpGet("flight-by-payment")]
        //[Authorize]
        public async Task<IActionResult> GetFlightByPayment([FromQuery] string paymentId)
        {
            if (string.IsNullOrWhiteSpace(paymentId))
            {
                return BadRequest("Payment ID cannot be null or empty.");
            }

            try
            {
                var flightDetails = await _ticketService.GetFlightByPaymentAsync(paymentId);
                return Ok(flightDetails);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[HttpGet("ticket-number")]
        ////[Authorize]
        //public async Task<IActionResult> GetTicketNumber([FromQuery] string flightID)
        //{
        //    if (string.IsNullOrWhiteSpace(flightID))
        //    {
        //        return BadRequest("Flight ID cannot be null or empty.");
        //    }

        //    try
        //    {
        //        var ticketNumber = await _ticketService.GetTicketNumberAsync(flightID);
        //        return Ok(new { ticketNumber });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}

        [HttpGet("passenger-tickets")]
        //[Authorize]
        public async Task<IActionResult> GetPassengerTickets([FromQuery] string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest("Username cannot be null or empty.");
            }

            try
            {
                var tickets = await _ticketService.GetPassengerTicketsAsync(username);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
