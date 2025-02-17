using AirLineSystem.Api.DTo;

namespace AirLineSystem.Api.Controllers
{
    public class GeneralHelper
    {
        public GeneralHelper() { }
        public List<string> checkInput(BookFlightRequest request)
        {

            var returnMessages = new List<string>();


            if (request.Amount <= 0)
            {
                returnMessages.Add("Invalid amount.");
            }


            if (string.IsNullOrWhiteSpace(request.Passenger) || request.Passenger.Length < 5)
            {
                returnMessages.Add("Invalid passenger account.");
            }


            if (string.IsNullOrWhiteSpace(request.Ticketnumber) || IsNumeric(request.Ticketnumber) || request.Ticketnumber.Length < 2)
            {
                returnMessages.Add("Invalid ticket number.");
            }


            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@") || !request.Email.Contains(".") || request.Email.Length < 5)
            {
                returnMessages.Add("Invalid email.");
            }


            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 5)
            {
                returnMessages.Add("Invalid name.");
            }


            if (string.IsNullOrWhiteSpace(request.seatnumber) || request.seatnumber.Length < 2 || request.seatnumber.Length > 3 || !char.IsDigit(request.seatnumber[0]) || (request.seatnumber.Length == 3 && !char.IsDigit(request.seatnumber[1])) || !char.IsLetter(request.seatnumber[^1]))
            {
                returnMessages.Add("Invalid seat number.");
            }


            if (string.IsNullOrWhiteSpace(request.FlightId) || !IsAlphanumeric(request.FlightId) || request.FlightId.Length < 4)
            {
                returnMessages.Add("Invalid flight ID.");
            }


            if (string.IsNullOrWhiteSpace(request.PhoneNumber) || !IsNumeric(request.PhoneNumber) || request.PhoneNumber.Length < 10 || request.PhoneNumber.Length > 15)
            {
                returnMessages.Add("Invalid phone number.");
            }

            return returnMessages;
        }

        private bool IsAlphanumeric(string input)
        {
            foreach (char c in input)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsNumeric(string input)
        {
            foreach (char c in input)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
