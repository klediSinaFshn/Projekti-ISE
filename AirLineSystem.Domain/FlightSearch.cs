using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AirLineSystem.Domain.Models
{
    public class FlightSearch
    {
        private static readonly HttpClient client = new HttpClient();

        private const string apiUrl = "https://test.api.amadeus.com/v2/shopping/flight-offers";
        //private const string apiKey = "43JKjoS6wfeHeAH0frF37OaiSUIu"; 


        private const string apiAuthUrl = "https://test.api.amadeus.com/v1/security/oauth2/token";
        private const string clientId = "NMynjXRjJvelkch6AzjJ3LXYy8nLqKly"; 
        private const string clientSecret = "TNuLeFrMoeGJZILu"; 


        private async Task<string> GetAuthTokenAsync()
        {
            var requestData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret)
            });


            HttpResponseMessage response = await client.PostAsync(apiAuthUrl, requestData);
            Console.WriteLine(response);
            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            var tokenData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            Console.WriteLine(tokenData);
            return tokenData.access_token;
        }

        // Method to get flight offers from Amadeus API
        public async Task<string> GetFlightOffersAsync(string origin, string destination, string departureDate, int adults)
        {

            try
            {
                // Get the auth token
                string token = await GetAuthTokenAsync();

                // Build the URL with query parameters
                string requestUrl = $"{apiUrl}?originLocationCode={origin}&destinationLocationCode={destination}&departureDate={departureDate}&adults={adults}&max=20";

                // Set the Authorization header for Bearer token
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                Console.WriteLine(token);

                // Send the GET request
                HttpResponseMessage response = await client.GetAsync(requestUrl);

                // Check if the response was successful
                response.EnsureSuccessStatusCode();

                // Read the response body
                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Process and return the formatted data
                var flightDetails = ProcessFlightData(jsonResponse);
                return JsonConvert.SerializeObject(flightDetails, Newtonsoft.Json.Formatting.Indented);
            }
            catch (Exception ex)
            {
                // Handle errors
                return $"Error: {ex.Message}";
            }
        }

        // Helper method to process the flight data and format it
        private JObject ProcessFlightData(string jsonResponse)
        {
            JObject responseObject = JObject.Parse(jsonResponse);

            // Get the "data" array from the response
            JToken flightOffersToken = responseObject["data"];
            Console.WriteLine(flightOffersToken);
            // Check if the "data" is a valid array
            if (flightOffersToken == null || flightOffersToken.Type != JTokenType.Array)
            {
                Console.WriteLine("No flight offers found or 'data' is not an array.");
                return new JObject(); // Return an empty result
            }

            // Initialize a JArray to store the formatted flight offers
        
            var formattedOffers = new JArray();


            HashSet<string> uniqueFlightNumbers = new HashSet<string>();

            // Loop through each flight offer in the "data" array
            foreach (var flightOffer in flightOffersToken)
            {
                    string flightNumber = flightOffer["itineraries"]?[0]["segments"]?[0]["number"]?.ToString();

                    // Skip if flight number is already processed
                    if (uniqueFlightNumbers.Contains(flightNumber))
                        continue;

                    // Add the flight number to the HashSet
                        uniqueFlightNumbers.Add(flightNumber);

                    // Format each flight offer
                    var formattedOffer = new JObject
                    {
                        ["airline"] = flightOffer["itineraries"]?[0]["segments"]?[0]["carrierCode"]?.ToString(),
                        ["flightNumber"] = flightNumber,
                        ["seatsAvailable"] = flightOffer["numberOfBookableSeats"]?.ToString() ?? "N/A",
                        ["departureTime"] = FormatDateTime(flightOffer["itineraries"]?[0]["segments"]?[0]["departure"]?["at"]?.ToString()),
                        ["arrivalTime"] = FormatDateTime(flightOffer["itineraries"]?[0]["segments"]?[0]["arrival"]?["at"]?.ToString()),
                        ["flightDuration"] = FormatDuration(flightOffer["itineraries"]?[0]["segments"]?[0]["duration"]?.ToString()),
                        ["price"] = flightOffer["price"]?["total"]?.ToString()
                    };

                    // Add the formatted offer to the formattedOffers array
                    formattedOffers.Add(formattedOffer);               

            }

            //Console.WriteLine(formattedOffers);

            // Return the final result as a JObject containing the formatted flight offers
            return new JObject
            {
                ["flights"] = formattedOffers
            };
        }
              

        private static string FormatDateTime(string dateTimeString)
        {
            if (DateTime.TryParse(dateTimeString, out DateTime dateTime))
            {
                return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            return "Invalid Date";
        }

        // Helper method to format duration (e.g., PT1H10M to 1 hour 10 minutes)
        private string FormatDuration(string isoDuration)
        {
            if (string.IsNullOrEmpty(isoDuration))
            {
                return "Invalid Duration";
            }

            // Remove leading "P" from the duration string
            var duration = isoDuration.TrimStart('P').ToUpperInvariant();

            // Initialize variables for hours and minutes
            int hours = 0;
            int minutes = 0;

            // Use regex to extract hours and minutes
            var hoursMatch = Regex.Match(duration, @"(\d+)H");
            if (hoursMatch.Success)
            {
                hours = int.Parse(hoursMatch.Groups[1].Value);
            }

            var minutesMatch = Regex.Match(duration, @"(\d+)M");
            if (minutesMatch.Success)
            {
                minutes = int.Parse(minutesMatch.Groups[1].Value);
            }

            // Return formatted duration string
            return $"{hours} hour{(hours > 1 ? "s" : "")} {minutes} minute{(minutes > 1 ? "s" : "")}";


        }

    }

}
