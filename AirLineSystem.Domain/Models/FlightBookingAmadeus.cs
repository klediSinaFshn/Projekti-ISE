using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AirLineSystem.Domain.Models
{
    public class FlightBookingAmadeus
    {
        private static readonly HttpClient client = new HttpClient();

        private const string apiBookingUrl = "https://test.api.amadeus.com/v1/booking/flight-orders";

        public string Arrival { get; set; }
        public string Departure { get; set; }
        public string DepartureDate { get; set; }

        public int FlightSelectionIndex { get; set; }

        public readonly FlightSearch flightSearch;

        public FlightBookingAmadeus()
        {
            this.flightSearch = new FlightSearch();
        }


        public void SetFlightDetails(string arrival, string departure, string departureDate)
        {
            Arrival = arrival;
            Departure = departure;
            DepartureDate = departureDate;
            //FlightSelectionIndex = flightSelectionIndex;
            Console.WriteLine(Arrival);
        }



        public async Task<string> CreateBookingAsync(int flightSelection, int adults, string firstName, string lastName, string email, string phone)
        {
            try
            {
               

                string flightOffersJson = await this.flightSearch.GetFlightOffersAsyncAmadeus(Departure, Arrival, DepartureDate, 1);
                Console.WriteLine($"Flight Offers JSON: {flightOffersJson}");

                JObject flightOffers = JObject.Parse(flightOffersJson);


                if (!flightOffers.ContainsKey("data"))
                {
                    Console.WriteLine("Error: No 'data' property found in the flight offers response.");
                    throw new Exception("No flight offers available.");
                }

                JArray flightsArray = flightOffers["data"] as JArray;
                if (flightsArray == null || flightSelection < 0 || flightSelection >= flightsArray.Count)
                {
                    Console.WriteLine($"Error: Invalid flight selection index. Available flights: {flightsArray?.Count ?? 0}");
                    throw new Exception("Invalid flight selection index.");
                }

                JToken selectedOffer = flightsArray[flightSelection];
                //Console.WriteLine("Selected Flight Offer:");
                //Console.WriteLine(selectedOffer.ToString());

               
                string token = await this.flightSearch.GetAuthTokenAsync();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                
                var bookingRequest = new
                {
                    data = new
                    {
                        type = "flight-order",
                        flightOffers = new object[] { selectedOffer },
                        travelers = new[]
                        {
                new
                {
                    id = "1",
                    dateOfBirth = "1982-01-16",
                    name = new { firstName = firstName, lastName = lastName },
                    gender = "MALE",
                    contact = new
                    {
                        emailAddress = email,
                        phones = new[]
                        {
                            new { deviceType = "MOBILE", countryCallingCode = "1", number = phone }
                        }
                    }
                }
            }
                    }
                };

                string jsonRequest = Newtonsoft.Json.JsonConvert.SerializeObject(bookingRequest, Newtonsoft.Json.Formatting.Indented);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

               
                HttpResponseMessage response = await client.PostAsync(apiBookingUrl, content);

               
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    //Console.WriteLine($"Response JSON: {jsonResponse}");

                    
                    JObject responseObject = JObject.Parse(jsonResponse);

                    string responseId = responseObject["data"]?["id"]?.ToString();

                    if (!string.IsNullOrEmpty(responseId))
                    {
                        Console.WriteLine($"Response : {jsonResponse}");
                    }
                    else
                    {
                        Console.WriteLine("Error: 'id' field is missing in the response.");
                    }

                    return responseId;
                }
                else
                {
                    
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error Response: {errorResponse}");
                    throw new HttpRequestException($"Error {response.StatusCode}: {errorResponse}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request Error: {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Parsing Error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                throw;
            }
        }

    }
}
