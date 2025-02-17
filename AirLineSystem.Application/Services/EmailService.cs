using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Policy;
using System.Text;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AirLineSystem.Services.Services
{
    public class EmailService
    {
        public string emailAdd {get; set;}
        public EmailService() {
            emailAdd = null;
        }
        public async Task<string> SendEmail(string email, string type,string paymentID)

        {
            Console.WriteLine(email+"email");
            var url_API = "https://pro.api.serversmtp.com/api/v2/authorize";
            var requestBody = @"{
                                  ""email"": ""elio.cena@fshnstudent.info"",
                                  ""password"": ""studeNt123!"",
                                  ""no_expire"": true
                                }";

            using var client = new HttpClient();
            var contentAPI = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var responseAPI = await client.PostAsync(url_API, contentAPI);
            var responseContent = await responseAPI.Content.ReadAsStringAsync();

            var consumerKey = "elio.cena@fshnstudent.info";
            var consumerSecret = "studeNt123!";

            string url = "https://api.turbo-smtp.com/api/v2/mail/send";

            var mailData = new
            {
                from = consumerKey,
                to = $"{emailAdd}",
                subject = "",
                content = "",
                html_content = ""
            };

            switch (type.ToLower())
            {
                case "booking":
                    mailData = new
                    {
                        from = consumerKey,
                        to = $"{emailAdd}",
                        subject = "Your Ticket has been booked",
                        content = "Your ticket has been booked. Thank you for choosing us and have a nice flight",
                        html_content = $"Your ticket has been booked. Thank you for choosing us and have a nice flight. Your payment ID is {paymentID} "
                    };
                    break;

                case "refund":
                    mailData = new
                    {
                        from = consumerKey,
                        to = $"{email}",
                        subject = "Your Refund has been processed",
                        content = "Your refund request has been processed successfully. Thank you for your understanding.",
                        html_content = $"Your refund request has been processed successfully for payment ID {paymentID}. Thank you for your understanding."
                    };
                    break;

                default:                    
                    
                    throw new ArgumentException("Invalid email type");
            }

            try
            {

                using (HttpClient httpClient = new HttpClient())
                {
                    // JSON data seriaization
                    var json = JsonSerializer.Serialize(mailData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Set authentication headers
                    content.Headers.Add("consumerKey", consumerKey);
                    content.Headers.Add("consumerSecret", consumerSecret);

                    // Trigger POST request
                    using (var response = await httpClient.PostAsync(url, content))
                    {
                            var result = await response.Content.ReadAsStringAsync();
                            Console.WriteLine("Response: " + result);

                        return result;

                    }
                }
            }
            catch (Exception ex) {
                return $"Something went wrong {ex}";
            }
        }

    }
}