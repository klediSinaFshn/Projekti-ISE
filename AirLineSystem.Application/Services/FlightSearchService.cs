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
    private const string apiKey = "ODgTt8k7jy1xbAcPyhnZo9tKeRQO";


    public async Task<string> GetFlightOffersAsync(string origin, string destination, string departureDate, int adults)
    {

        try
        {
           
            string requestUrl = $"{apiUrl}?originLocationCode={origin}&destinationLocationCode={destination}&departureDate={departureDate}&adults={adults}&max=2";

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");


            HttpResponseMessage response = await client.GetAsync(requestUrl);
            //Console.WriteLine(response);
         
            response.EnsureSuccessStatusCode();

            
            string jsonResponse = await response.Content.ReadAsStringAsync();


            
            var flightDetails = ProcessFlightData(jsonResponse);

            return JsonConvert.SerializeObject(flightDetails, Newtonsoft.Json.Formatting.Indented);
        }
        catch (Exception ex)
        {
         
            return $"Error: {ex.Message}";
        }
    }

   
    private JObject ProcessFlightData(string jsonResponse)
    {
        JObject responseObject = JObject.Parse(jsonResponse);

    
        JToken flightOffersToken = responseObject["data"];
          //Console.WriteLine(flightOffersToken);
      
        if (flightOffersToken == null || flightOffersToken.Type != JTokenType.Array)
        {
            Console.WriteLine("No flight offers found or 'data' is not an array.");
            return new JObject(); 
        }

    
        var formattedOffers = new JArray();

      
        foreach (var flightOffer in flightOffersToken)
        {
            
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

            Console.WriteLine(formattedOffer);
          
            formattedOffers.Add(formattedOffer);
        }

        Console.WriteLine(formattedOffers);

      
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

  
    private string FormatDuration(string isoDuration)
    {
        if (string.IsNullOrEmpty(isoDuration))
        {
            return "Invalid Duration";
        }

        var duration = isoDuration.TrimStart('P').ToUpperInvariant();

        int hours = 0;
        int minutes = 0;

      
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

        return $"{hours} hour{(hours > 1 ? "s" : "")} {minutes} minute{(minutes > 1 ? "s" : "")}";


    }

}





