using Npgsql;
using AirLineSystem.Domain.Models;
using Npgsql;
using System.Data;
using System.Text.Json;
using System.Data.Common;

namespace AirLineSystem.Domain.Models
{
    public class Ticket
    {
     

        private readonly NpgsqlConnection _dbConnection;
      
        //Constructor
        public Ticket( NpgsqlConnection dbConnection) { 
            
            _dbConnection = dbConnection;
        }

        public async Task<JsonDocument> GetFlightByPaymentAsync(string paymentId)
        {
            try
            {
                if (_dbConnection == null)
                {
                    throw new InvalidOperationException("Database connection is not initialized.");
                }

                if (string.IsNullOrWhiteSpace(paymentId))
                {
                    throw new ArgumentException("Payment ID cannot be null or empty.", nameof(paymentId));
                }

                // Proper query without unnecessary quotes around the parameter
                await using var command = new NpgsqlCommand("SELECT \"id\", \"user\", \"payment_amount\", \"ticket_number\", \"email\", \"status\", \"name\", \"payment_ID\", \"flightID\", \"phone_number\",\"bookingID\" FROM booked_tickets.tickets WHERE \"payment_ID\" = @payment_ID and \"status\"='P'", _dbConnection);
                command.Parameters.AddWithValue("@payment_ID", paymentId);

                await using var reader = await command.ExecuteReaderAsync();

                if (!reader.HasRows)
                {
                    throw new InvalidOperationException($"No flight found for Payment ID: {paymentId}");
                }

                // Create a list to hold the results
                var flightDataList = new List<Dictionary<string, object>>();

                // Read through each row in the result set
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>
            {
                { "id", reader["id"] },
                { "user", reader["user"] },
                { "payment_amount", reader["payment_amount"] },
                { "ticket_number", reader["ticket_number"] },
                { "email", reader["email"] },
                { "status", reader["status"] },
                { "name", reader["name"] },
                { "payment_ID", reader["payment_ID"] },
                { "flightID", reader["flightID"] },
                { "phone_number", reader["phone_number"] },
                { "bookingID", reader["bookingID"] }
            };

                    flightDataList.Add(row);
                }

                // Convert the result into a JSON document
                var jsonResponse = JsonSerializer.Serialize(flightDataList);
                return JsonDocument.Parse(jsonResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving flight for payment ID: {paymentId}. Exception: {ex.Message}");
                throw;
            }
            finally
            {
                await _dbConnection.CloseAsync();
            }
        }
      

        public async Task<JsonDocument> GetPassengerTickets(string user) // needed for ticket retrive based on the username of the passenger.
        {
            await using var command = new NpgsqlCommand(
                "SELECT  \"user\", ticket_number, payment_amount, status, email, name, \"flightID\", \"payment_ID\" " +
                "FROM booked_tickets.tickets " +
                "WHERE \"user\" = @user and \"status\"='P'", _dbConnection);

            command.Parameters.AddWithValue("@user", user);

            var tickets = new List<Dictionary<string, object>>();

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                        var ticket = new Dictionary<string, object>
                 {            
                    { "user", reader["user"] },
                    { "ticket_number", reader["ticket_number"] },
                    { "payment_amount", reader["payment_amount"] },
                    { "status", reader["status"] },
                    { "email", reader["email"] },
                    { "name", reader["name"] },
                    { "flightID", reader["flightID"] },
                    { "payment_ID", reader["payment_ID"] }
                };

                tickets.Add(ticket);
            }

            await _dbConnection.CloseAsync();

            var json = JsonSerializer.Serialize(tickets);
            return JsonDocument.Parse(json);

        }

    }
}