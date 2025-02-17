using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AirLineSystem.Domain.Models;
using Npgsql;
using System.Data;
using System.Text.Json;
using System.Data.Common;

namespace AirLineSystem.Domain.Models
{
    public class Passenger
    {
        //Class Properties
        //public string PassengerUser{ get; set; }
        //public string Name {  get; set; }
        //public string Email { get; set; }
        //public string PhoneNumber { get; set; }
        //public List<Ticket> Tickets { get; set; }

        public readonly NpgsqlConnection _dbConnection;

        //Constructor
        public Passenger( NpgsqlConnection dbConnection)
        {
         
            _dbConnection = dbConnection;

        }

        public async Task<JsonDocument> GetPassengerDetails(string user) {
            if (string.IsNullOrWhiteSpace(user))
            {
                throw new ArgumentException("User cannot be null or empty.", nameof(user));
            }

            await _dbConnection.OpenAsync();

            await using var command = new NpgsqlCommand("SELECT  \"user\",  email, name ,phone_number  FROM booked_tickets.tickets\r\nWHERE \"user\" = '@user'", _dbConnection);
            command.Parameters.AddWithValue("@user", user);

            var result = await command.ExecuteScalarAsync();

            if (result == null)
            {
                throw new InvalidOperationException($"No flight found for User: {user}");
            }

            await _dbConnection.CloseAsync();
            // Parse the result as JSON
            return JsonDocument.Parse(result.ToString());

        }

     

    }
}
