using AirLineSystem.Domain.Models;
using Npgsql;
using System.Data;

namespace AirLineSystem.Domain.Models;
public class BookFlights
{
    private readonly NpgsqlConnection _dbConnection;

    public BookFlights(NpgsqlConnection dbConnection)
    {
        _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection), "Database connection cannot be null");
    }

    public async Task<string> BookFlightAsync(decimal amount, string user,string payment_ID, string ticket_number, string email,string name,string flightID,string phone_number)
    {
        try
        {
            if (_dbConnection.State != ConnectionState.Open)
                await _dbConnection.OpenAsync();

            
            string insertQuery = "INSERT INTO booked_tickets.tickets (\"user\",\"payment_amount\",\"ticket_number\",\"email\",\"status\",\"name\",\"payment_ID\",\"flightID\",\"phone_number\") VALUES (@user,@amount,@ticket_number,@email,@status,@name,@payment_ID,@flightID,@phone_number)";
            using (var command = new NpgsqlCommand(insertQuery, _dbConnection))
            {
                command.Parameters.AddWithValue("@user", user);
                command.Parameters.AddWithValue("@amount", amount);
                command.Parameters.AddWithValue("@ticket_number", ticket_number);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@status", "B");
                command.Parameters.AddWithValue("@payment_ID", payment_ID);
                command.Parameters.AddWithValue("@flightID", flightID);           
                command.Parameters.AddWithValue("@phone_number",phone_number);
                command.Parameters.AddWithValue("@name",name);
                
                await command.ExecuteNonQueryAsync();
            }

            await _dbConnection.CloseAsync();
            return ("Booking successful");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return $"Error booking flight: {ex.Message}";
        }
    }

    public async Task<string> CancelBookingAsync(string bookingID)
    {
        try
        {
            if (_dbConnection.State != ConnectionState.Open)
                await _dbConnection.OpenAsync();

            string updateQuery = "UPDATE flight_bookings SET \"status\" = @status WHERE \"bookingID\" = @bookingID";
            using (var command = new NpgsqlCommand(updateQuery, _dbConnection))
            {
                command.Parameters.AddWithValue("@status", "Canceled");
                command.Parameters.AddWithValue("@bookingID", bookingID);
                await command.ExecuteNonQueryAsync();
            }
            await _dbConnection.CloseAsync();
            return "Booking canceled successfully";
        }

        catch (Exception ex)
        {
            await _dbConnection.CloseAsync();
            
            return $"Error canceling booking: {ex.Message}";
        }
    }

    public async Task<string> ConfirmBooking(string paymentID,string bookingID) {
        try
        {
            if (_dbConnection.State != ConnectionState.Open)
                await _dbConnection.OpenAsync();

            string updateQuery = "UPDATE booked_tickets.tickets SET \"status\" = @status , \"bookingID\" = @bookingID WHERE \"payment_ID\" = @paymentID";
            using (var command = new NpgsqlCommand(updateQuery, _dbConnection))
            {
                command.Parameters.AddWithValue("@status", "P");
                command.Parameters.AddWithValue("@paymentID", paymentID);
                command.Parameters.AddWithValue("@bookingID", bookingID);

                await command.ExecuteNonQueryAsync();
            }
            await _dbConnection.CloseAsync();
            return "Refund processed successfully";
        }
        catch (Exception ex)
        {
            await _dbConnection.CloseAsync();

            return $"Error processing refund: {ex.Message}";
        }
    }



    public async Task<string> RefundPaymentAsync(string paymentID, string bookingID, string email)
    {

        Console.WriteLine(paymentID + bookingID);
        try
        {
            if (_dbConnection.State != ConnectionState.Open)
                await _dbConnection.OpenAsync();

            string updateQuery = "UPDATE booked_tickets.tickets SET \"status\" = @status WHERE \"payment_ID\" = @paymentID and \"email\" =  @email";
            using (var command = new NpgsqlCommand(updateQuery, _dbConnection))
            {
                command.Parameters.AddWithValue("@status", "C");
                command.Parameters.AddWithValue("@paymentID", paymentID);
                command.Parameters.AddWithValue("@email",email);

                await command.ExecuteNonQueryAsync();
            }
            await _dbConnection.CloseAsync();
            return "Refund processed successfully";
        }
        catch (NpgsqlException npgsqlEx)
        {
            // Handle database-specific exceptions
            await _dbConnection.CloseAsync();
            return $"Database error while processing refund: {npgsqlEx.Message}";
        }
        catch (InvalidOperationException invalidOpEx)
        {
            // Handle invalid operation exceptions (e.g., connection issues)
            await _dbConnection.CloseAsync();
            return $"Invalid operation error: {invalidOpEx.Message}";
        }
        catch (Exception ex)
        {
            // General exception handler for unexpected errors
            await _dbConnection.CloseAsync();
            return $"Error processing refund: {ex.Message}";
        }

    }
}