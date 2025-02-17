using AirLineSystem.Domain.Models;
using AirLineSystem.Services.Interfaces;
using AirLineSystem.Services.Services;
using Microsoft.Extensions.DependencyInjection;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirLineSystem.DependencyContainer
{
    public static class DependencyContainer
    {
        public static void RegisterServices(IServiceCollection services) {
            services.AddSingleton<Domain.Models.DbConnection>();
            services.AddScoped<Domain.Models.FlightBookingAmadeus>();
            services.AddSingleton<IBookingService, BookingService>();
            services.AddScoped<ITicketService, TicketService>();           
            services.AddScoped<IPaymentProcess, PaymentService>();           
            services.AddScoped<StripeService>();
            services.AddScoped<EmailService>();
        }
    }
}
