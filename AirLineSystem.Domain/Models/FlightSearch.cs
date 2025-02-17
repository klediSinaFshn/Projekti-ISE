using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using AirLineSystem.Domain.Models;

public class FlightSearch
{
    private static readonly HttpClient client = new HttpClient();
    private const string apiUrl = "https://test.api.amadeus.com/v2/shopping/flight-offers";
    private const string apiAuthUrl = "https://test.api.amadeus.com/v1/security/oauth2/token";
    private const string clientId = "NMynjXRjJvelkch6AzjJ3LXYy8nLqKly";
    private const string clientSecret = "TNuLeFrMoeGJZILu";

    private static string cachedToken = null;
    private static DateTime tokenExpiry = DateTime.MinValue;
    private static readonly SemaphoreSlim tokenSemaphore = new SemaphoreSlim(1, 1);
    public string departureToCall;
    public string arrivalToCall;
    public string departureDateToCall;
    public FlightBookingAmadeus flightBookingAmadeus;


    public async Task<string> GetAuthTokenAsync()
    {
        
        if (cachedToken != null && DateTime.UtcNow < tokenExpiry)
        {
            return cachedToken;
        }

        await tokenSemaphore.WaitAsync();
        try
        {
          
            if (cachedToken != null && DateTime.UtcNow < tokenExpiry)
            {
                return cachedToken;
            }

            var requestData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret)
            });

            HttpResponseMessage response = await client.PostAsync(apiAuthUrl, requestData);
            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            var tokenData = JsonConvert.DeserializeObject<dynamic>(responseContent);

            cachedToken = tokenData.access_token;
            tokenExpiry = DateTime.UtcNow.AddSeconds((int)tokenData.expires_in);

            return cachedToken;
        }
        finally
        {
            tokenSemaphore.Release();
        }
    }

    public async Task<string> GetFlightOffersAsync(string origin, string destination, string departureDate, int adults)
    {
        try
        {
            string token = await GetAuthTokenAsync();
            string requestUrl = $"{apiUrl}?originLocationCode={origin}&destinationLocationCode={destination}&departureDate={departureDate}&adults={adults}&max=20";
                        
           
            


            client.DefaultRequestHeaders.Clear();

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            HttpResponseMessage response = await client.GetAsync(requestUrl);

            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var flightDetails = ProcessFlightData(jsonResponse);
            departureToCall = origin;
            arrivalToCall = destination;
            departureDateToCall = departureDate;
            return JsonConvert.SerializeObject(flightDetails, Newtonsoft.Json.Formatting.Indented);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    public async Task<string> GetFlightOffersAsyncAmadeus(string origin, string destination, string departureDate, int adults)
    {
        try
        {
            string token = await GetAuthTokenAsync();
            string requestUrl = $"{apiUrl}?originLocationCode={origin}&destinationLocationCode={destination}&departureDate={departureDate}&adults={adults}&max=20";

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            HttpResponseMessage response = await client.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            string jsonResponse = await response.Content.ReadAsStringAsync();

           
            Console.WriteLine("Raw Response: " + jsonResponse);

        
            var parsedResponse = JToken.Parse(jsonResponse);
            return JsonConvert.SerializeObject(parsedResponse, Formatting.Indented);
        }
        catch (JsonReaderException ex)
        {
            return $"JSON Parsing Error: {ex.Message}";
        }
        catch (HttpRequestException ex)
        {
            return $"HTTP Request Error: {ex.Message}";
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

        if (flightOffersToken == null || flightOffersToken.Type != JTokenType.Array)
        {
            return new JObject();
        }

        var formattedOffers = new JArray();
        HashSet<string> uniqueFlightNumbers = new HashSet<string>();

        foreach (var flightOffer in flightOffersToken)
        {
            string flightNumber = flightOffer["itineraries"]?[0]["segments"]?[0]["number"]?.ToString();
            if (uniqueFlightNumbers.Contains(flightNumber))
                continue;

            uniqueFlightNumbers.Add(flightNumber);

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

            formattedOffers.Add(formattedOffer);
        }

        return new JObject { ["flights"] = formattedOffers };
    }

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
        int hours = 0, minutes = 0;

        var hoursMatch = Regex.Match(duration, @"(\d+)H");
        if (hoursMatch.Success)
            hours = int.Parse(hoursMatch.Groups[1].Value);

        var minutesMatch = Regex.Match(duration, @"(\d+)M");
        if (minutesMatch.Success)
            minutes = int.Parse(minutesMatch.Groups[1].Value);

        return $"{hours} hour{(hours > 1 ? "s" : "")} {minutes} minute{(minutes > 1 ? "s" : "")}";
    }
}
