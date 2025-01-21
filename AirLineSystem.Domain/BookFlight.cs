using Npgsql;
using System.Data;

public class BookFlights
{
    private readonly NpgsqlConnection _dbConnection;

    public BookFlights(NpgsqlConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<string> BookFlightAsync(decimal amount, string currency = "USD")
    {
        try
        {
            if (_dbConnection.State != ConnectionState.Open)
                await _dbConnection.OpenAsync();

            string insertQuery = "INSERT INTO flight_bookings (amount, currency, status) VALUES (@amount, @currency, @status)";
            using (var command = new NpgsqlCommand(insertQuery, _dbConnection))
            {
                command.Parameters.AddWithValue("@amount", amount);
                command.Parameters.AddWithValue("@currency", currency);
                command.Parameters.AddWithValue("@status", "Booked");
                await command.ExecuteNonQueryAsync();
            }

            return "Booking successful";
        }
        catch (Exception ex)
        {
            // Optionally, log the error here
            return $"Error booking flight: {ex.Message}";
        }
    }

    public async Task<string> CancelBookingAsync(string orderId)
    {
        try
        {
            if (_dbConnection.State != ConnectionState.Open)
                await _dbConnection.OpenAsync();

            string updateQuery = "UPDATE flight_bookings SET status = @status WHERE order_id = @orderId";
            using (var command = new NpgsqlCommand(updateQuery, _dbConnection))
            {
                command.Parameters.AddWithValue("@status", "Canceled");
                command.Parameters.AddWithValue("@orderId", orderId);
                await command.ExecuteNonQueryAsync();
            }

            return "Booking canceled successfully";
        }
        catch (Exception ex)
        {
            // Log error here
            return $"Error canceling booking: {ex.Message}";
        }
    }

    public async Task<string> RefundPaymentAsync(string captureId)
    {
        try
        {
            if (_dbConnection.State != ConnectionState.Open)
                await _dbConnection.OpenAsync();

            string updateQuery = "UPDATE flight_bookings SET status = @status WHERE capture_id = @captureId";
            using (var command = new NpgsqlCommand(updateQuery, _dbConnection))
            {
                command.Parameters.AddWithValue("@status", "Refunded");
                command.Parameters.AddWithValue("@captureId", captureId);
                await command.ExecuteNonQueryAsync();
            }

            return "Refund processed successfully";
        }
        catch (Exception ex)
        {
            // Log error here
            return $"Error processing refund: {ex.Message}";
        }
    }
}