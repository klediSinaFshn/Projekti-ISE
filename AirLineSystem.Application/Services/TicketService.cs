using AirLineSystem.Domain.Models;
using AirLineSystem.Services.Interfaces;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;

namespace AirLineSystem.Services.Services
{
    public class TicketService : ITicketService
    {
        private NpgsqlConnection _dbConnection;

        public TicketService()

        {
            _dbConnection = null;
        }

   
        public async Task InitializeAsync()
        {
            var dbService = new DbConnection();
            _dbConnection = await dbService.DbConnectionInit();
        }

        public static async Task<TicketService> CreateInstanceAsync()
        {
            var instance = new TicketService();
            await instance.InitializeAsync();
            return instance;
        }

        public async Task<JsonDocument> GetFlightByPaymentAsync(string paymentId)
        {
            if (string.IsNullOrWhiteSpace(paymentId))
                throw new ArgumentException("Payment ID cannot be null or empty.", nameof(paymentId));

            if (_dbConnection == null)
                await InitializeAsync();

            if (_dbConnection.State != ConnectionState.Open)
                await _dbConnection.OpenAsync();

            var ticket = new Ticket(_dbConnection);
            try
            {
                return await ticket.GetFlightByPaymentAsync(paymentId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving flight for payment ID: {paymentId}", ex);
            }
        }


        public async Task<JsonDocument> GetPassengerTicketsAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));

            if (_dbConnection == null)
                await InitializeAsync();

            if (_dbConnection.State != ConnectionState.Open)
                await _dbConnection.OpenAsync();

            var ticket = new Ticket(_dbConnection);
            try
            {
                return await ticket.GetPassengerTickets(username);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving tickets for user: {username}", ex);
            }
        }

        public async Task CloseConnectionAsync()
        {
            if (_dbConnection != null && _dbConnection.State == ConnectionState.Open)
            {
                await _dbConnection.CloseAsync();
            }
        }
    }
}
