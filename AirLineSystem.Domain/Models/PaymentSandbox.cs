using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Threading.Tasks;

namespace AirLineSystem.Domain.Models
{
    public class StripeService
    {
        private readonly string _secretKey;   


        public StripeService(IConfiguration configuration)
        {
            
            _secretKey = "sk_test_51QPgq1G8xzdmTSZJUYcit5YzhD7Ro835yN5xa7r29BGf2D8WzwiBziVGAGI1KobhWTHSvIFxKUUnutHdWX9RmCjJ00n8etUBMJ";

            
            if (string.IsNullOrEmpty(_secretKey))
            {
                throw new InvalidOperationException("Stripe Secret Key is missing in the configuration.");
            }

           
            StripeConfiguration.ApiKey = _secretKey;
        }

     
        public async Task<string> CreatePaymentIntentAsync(decimal amount)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100), 
                    Currency = "eur",
                    PaymentMethodTypes = new List<string> { "card" },
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                Console.WriteLine($"PaymentIntent ID: {paymentIntent.Id}, Client Secret {paymentIntent.ClientSecret}, Status: {paymentIntent.Status}");
                //return paymentIntent.ClientSecret;
                return paymentIntent.ClientSecret;
            }
            catch (Exception ex)
            {
                return $"Error creating payment intent: {ex.Message}";
            }
        
        }

      
        public async Task<string> RefundPaymentAsync(string paymentIntentId)
        {
            try
            {
                var service = new RefundService();
                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId,
                };
                var refund = await service.CreateAsync(refundOptions);

                return $"Refund successful. Refund ID: {refund.Id}";
            }
            catch (Exception ex)
            {
                return $"Error processing refund: {ex.Message}";
            }
        }
    }
}