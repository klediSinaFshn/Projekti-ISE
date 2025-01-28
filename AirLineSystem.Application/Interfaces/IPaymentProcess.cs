using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirLineSystem.Services.Interfaces
{
    public interface IPaymentProcess
    {
        Task<string> CreateOrderAsync(decimal amount);
        Task<string> RefundPaymentAsync(string paymentIntentId);
    }
}
