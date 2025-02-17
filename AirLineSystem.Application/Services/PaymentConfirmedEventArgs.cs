using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirLineSystem.Services.Services
{
    public class PaymentConfirmedEventArgs : EventArgs
    {
        public string Type { get; }
        public string Email { get; }
        public string PaymentID { get; }



        public PaymentConfirmedEventArgs(string email, string type,string confirmationID)
        {
            Type = type;
            Email = email;
            PaymentID = confirmationID;
           
        }

    }
}
