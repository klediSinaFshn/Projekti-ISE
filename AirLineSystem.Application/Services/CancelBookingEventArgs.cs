using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirLineSystem.Services.Services
{
    public class CancelBookingEventArgs : EventArgs
    {
        public string Email { get; }
        public string Type { get; }
        public string PaymentID { get; }

        public CancelBookingEventArgs(string email, string type, string bookingID)
        {
            Email = email;
            Type = type;
            PaymentID = bookingID;
        }
    }
}
