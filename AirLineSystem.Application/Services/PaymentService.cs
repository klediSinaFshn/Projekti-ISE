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

        // Constructor to initialize PayPal service
        public PaymentService()
        {
           
            _stripeService = new AirLineSystem.Domain.Models.StripeService(configuration);
        }

        // Create a new order (buy action)
        public async Task<string> CreateOrderAsync(decimal amount)
        {
            return await _stripeService.CreatePaymentIntentAsync(amount);
        }

        
        // Refund a captured payment
        public async Task<string> RefundPaymentAsync(string paymentIntentId)
        {
            return await _stripeService.RefundPaymentAsync(paymentIntentId);
        }
    }



}







