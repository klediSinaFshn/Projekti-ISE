using System.Text.Json;
using System.Threading.Tasks;

namespace AirLineSystem.Services.Interfaces
{
    public interface ITicketService
    {
        
        Task<JsonDocument> GetFlightByPaymentAsync(string paymentId);        
        Task<JsonDocument> GetPassengerTicketsAsync(string username);        
        Task CloseConnectionAsync();
    }
}
