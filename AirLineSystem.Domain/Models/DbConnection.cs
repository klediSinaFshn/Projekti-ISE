using AirLineSystem.Domain.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace AirLineSystem.Domain.Models
{

    public class DbConnection
    {

        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly string _database;


        public DbConnection()
        {
            _host = "127.0.0.1";
            _port = 5432;
            _username = "postgres";
            _password = "Admin1234";
            _database = "postgres";
        }

        public async Task<NpgsqlConnection> DbConnectionInit()
        {
            try
            {
                string connectionString = $"Host={_host};Port={_port};Username={_username};Password={_password};Database={_database};";

                var connection = new NpgsqlConnection(connectionString);

                await connection.OpenAsync();

                using (var cmd = new NpgsqlCommand("SET search_path TO booked_tickets;", connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                return connection;
            }
            catch (NpgsqlException ex)
            {
                // Log or display detailed database connection error information
                string errorMessage = $"Database connection failed: {ex.Message}";
                string innerErrorMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception.";
                string stackTrace = ex.StackTrace;

                // Optionally log the error to a file or a monitoring system
                Console.WriteLine($"Error: {errorMessage}\nInner Error: {innerErrorMessage}\nStack Trace: {stackTrace}");

                // You can also throw the exception or rethrow it depending on your needs
                throw new InvalidOperationException("Failed to establish a database connection.", ex);
            }
            catch (Exception ex)
            {
                // Handle other exceptions that are not related to Npgsql
                Console.WriteLine($"General Error: {ex.Message}\nStack Trace: {ex.StackTrace}");
                throw;  // Optionally rethrow the exception
            }
        }
    }

}