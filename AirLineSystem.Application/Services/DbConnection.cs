using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AirLineSystem.Services.Services
{
   
    public class DbConnection
    {
        //private const string dbconnection = "Host=127.0.0.1;Port=5432;Username=Admin;Password=Aloha123;Database=booked_tickets";
        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly string _database;


        public DbConnection()
        {
            _host = "127.0.0.1";
            _port = 5432;
            _username = "Admin";
            _password = "Aloha123";
            _database = "booked_tickets";
        }

        public async Task<NpgsqlConnection> DbConnectionInit()
        {
            try
            {
                string connectionString = $"Host={_host};Port={_port};Username={_username};Password={_password};Database={_database};";

                var connection = new NpgsqlConnection(connectionString);

                await connection.OpenAsync();

                return connection;
            }
            catch (Exception ex) {

                throw new InvalidOperationException("Failed to establish a database connection.", ex);

            }
        }
    }
}
