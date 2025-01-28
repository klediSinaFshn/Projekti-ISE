using System.Globalization;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Xml.Linq;
using System.Xml;
using System.Text.RegularExpressions;
public class FlightSearchService
{
    private static readonly HttpClient client = new HttpClient();

    private const string apiUrl = "https://test.api.amadeus.com/v2/shopping/flight-offers";
    private const string apiKey = "ODgTt8k7jy1xbAcPyhnZo9tKeRQO"; // Replace with your actual API key

    // Method to get flight offers from Amadeus API
    public async Task<string> GetFlightOffersAsync(string origin, string destination, string departureDate, int adults)
    {

        try
        {
            // Build the URL with query parameters
            string requestUrl = $"{apiUrl}?originLocationCode={origin}&destinationLocationCode={destination}&departureDate={departureDate}&adults={adults}&max=2";

            // Set the Authorization header for API Key (or Bearer token if necessary)
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");


            // Send the GET request
            HttpResponseMessage response = await client.GetAsync(requestUrl);
            //Console.WriteLine(response);
            // Check if the response was successful
            response.EnsureSuccessStatusCode();

            // Read the response body
            string jsonResponse = await response.Content.ReadAsStringAsync();


            // Process the response and format it
            var flightDetails = ProcessFlightData(jsonResponse);

            // Return the formatted data as a JSON string
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

        // Loop through each flight offer in the "data" array
        foreach (var flightOffer in flightOffersToken)
        {
            // Format each flight offer
            var formattedOffer = new JObject
            {
                ["airline"] = flightOffer["itineraries"]?[0]["segments"]?[0]["carrierCode"]?.ToString(),
                ["flightNumber"] = flightOffer["itineraries"]?[0]["segments"]?[0]["number"]?.ToString(),
                ["seatsAvailable"] = flightOffer["numberOfBookableSeats"]?.ToString() ?? "N/A",
                ["departureTime"] = FormatDateTime(flightOffer["itineraries"]?[0]["segments"]?[0]["departure"]?["at"]?.ToString()),
                ["arrivalTime"] = FormatDateTime(flightOffer["itineraries"]?[0]["segments"]?[0]["arrival"]?["at"]?.ToString()),
                ["flightDuration"] = FormatDuration(flightOffer["itineraries"]?[0]["segments"]?[0]["duration"]?.ToString()),
                ["price"] = flightOffer["price"]?["total"]?.ToString()
            };

            // Add the formatted offer to the formattedOffers array
            formattedOffers.Add(formattedOffer);
        }

        Console.WriteLine(formattedOffers);

        // Return the final result as a JObject containing the formatted flight offers
        return new JObject
        {
            ["flights"] = formattedOffers
        };
    }

    // Helper method to format ISO 8601 datetime to a more readable format
    //private string FormatDateTime(string isoDateTime)
    //{
    //    DateTime dateTime = DateTime.Parse(isoDateTime, null, DateTimeStyles.RoundtripKind);
    //    return dateTime.ToString("dd MMM yyyy, HH:mm", CultureInfo.InvariantCulture);
    //}


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





