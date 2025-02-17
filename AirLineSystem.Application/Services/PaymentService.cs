using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using AirLineSystem.Domain.Models;
using Microsoft.Extensions.Configuration;
using Stripe;
using AirLineSystem.Services.Interfaces;

namespace AirLineSystem.Services.Services
{
    public class PaymentService : IPaymentProcess
    {
       
        private readonly AirLineSystem.Domain.Models.StripeService _stripeService;
       
        private IConfiguration configuration;

        
        public PaymentService()
        {
           
            _stripeService = new AirLineSystem.Domain.Models.StripeService(configuration);
         
        }

        public async Task<string> CreateOrderAsync(decimal amount)
        {
            return await _stripeService.CreatePaymentIntentAsync(amount);
        }

        
        public async Task<string> RefundPaymentAsync(string paymentIntentId)
        {
            try
            {
                return await _stripeService.RefundPaymentAsync(paymentIntentId);
            }
            catch (Exception ex) {
                return $"Soemthing went wrong: {ex.Message}";
            }
        }
    }



}







