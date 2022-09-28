using System.Diagnostics;

// Coderpad Solution all-in-one file
// To execute C#, please define "static void Main" on a class
// named Solution.
public struct Event
{
    public String City;
    public String Name;
    public Double Price;
}


public struct City
{
    public string Name;
    public Coordinate Coordinate;

}

public class Coordinate
{
    public double Latitude;
    public double Longitude;
    private const double nearest = 5.0;

    public Double NearestLatitude()
    {
        return Math.Round(this.Latitude / nearest) * (int)nearest;
    }

    public Double NearestLongtitude()
    {
        return Math.Round(this.Longitude / nearest) * (int)nearest;
    }

}


public struct Journey
{
    public Coordinate start;
    public Coordinate end;
}



public struct Customer
{
    public string Name;
    public string City;
}

public static class NumericExtensions
{
    public static double ToRadians(this double val)
    {
        return (Math.PI / 180) * val;
    }
}

class Solution
{
    private static Dictionary<string, int> journeyDistance = new Dictionary<string, int>();
    private static List<Event> pricedEvents = new List<Event>();

    private static List<Event> events = new List<Event>{
            new Event{ Name = "Phantom of the Opera", City =  "New York"},
            new Event{ Name = "Metallica", City = "Los Angeles"},
            new Event{ Name = "Metallica", City = "New York"},
            new Event{ Name = "Metallica", City = "Boston"},
            new Event{ Name = "LadyGaGa", City = "New York"},
            new Event{ Name = "LadyGaGa", City = "Boston"},
            new Event{ Name = "LadyGaGa", City = "Chicago"},
            new Event{ Name = "LadyGaGa", City = "San Francisco"},
            new Event{ Name = "LadyGaGa", City = "Washington DC"}
        };

    private static List<City> cities = new List<City>{
            new City{Name = "New York", Coordinate = new Coordinate{ Latitude = 40.730610, Longitude = -73.935242 } },
            new City{Name = "Boston", Coordinate = new Coordinate{ Latitude = 42.361145, Longitude = -71.057083 } },
            new City{Name = "Chicago", Coordinate = new Coordinate{ Latitude = 41.510395, Longitude = -87.644287} },
            new City{Name = "Washington DC", Coordinate = new Coordinate{ Latitude = 38.900497, Longitude = -77.007507 } },
            new City{Name = "Los Angeles", Coordinate = new Coordinate{ Latitude = 34.052235, Longitude = -118.243683 } },
            new City{Name = "San Francisco", Coordinate = new Coordinate{ Latitude = 37.773972, Longitude = -122.431297 } },
        };

    private static List<Customer> customers = new List<Customer> {
            new Customer { Name = "Angel Lopez", City = "Los Angeles" },
            new Customer { Name = "Joe Soap", City = "Boston"},
            new Customer { Name = "Mark Crow", City = "New York"},
            new Customer { Name = "George Perez", City = "Washington DC"},
            new Customer { Name = "Matthew Luke", City="Los Angeles"},
            new Customer { Name = "Guy Wise", City = "Chicago"},
            new Customer { Name = "Frank Saint", City = "San Francisco"},
            new Customer { Name = "Oliver Rock", City = "Boston"}
        };

    static void Main(string[] args)
    {
        Stopwatch processTimer = new Stopwatch();

        processTimer.Start();
        GenerateEventPrices();

        foreach (Customer customer in customers)
        {
            PrepareCustomerEmail(customer);
        }
        processTimer.Stop();
        Console.WriteLine("Process took {0} ms.", processTimer.ElapsedMilliseconds);
    }
    private static void LogEmailHeader(Customer customer)
    {
        Console.WriteLine("Composing Email For: {0} in {1}", customer.Name, customer.City);
        Console.WriteLine("---------------------------------");
    }

    private static void LogShowsHeader(Customer customer)
    {
        Console.WriteLine("Shows For: {0} in {1}", customer.Name, customer.City);
        Console.WriteLine("---------------------------------");
    }

    public static void PrepareCustomerEmail(Customer customer)
    {
        // collect all events in  customer city
        LogEmailHeader(customer);
        ClosestEvents(customer, 5);

    }

    public static void ClosestEvents(Customer customer, int noOfEvents)
    {

        // Get n nearest shows to customer
        var shows = pricedEvents.Select(e => new { Distance = GetCachedDistance(customer.City, e.City), Event = e, Price = e.Price })
            .OrderBy(e => e.Distance).ThenBy(e => e.Price).Take(noOfEvents).ToList();

        LogShowsHeader(customer);
        shows.ForEach(s =>
            AddToEmail(customer, s.Event));
        Console.WriteLine();
    }

    public static void AddToEmail(Customer customer, Event @event)
    {
        // This left blank
        Console.WriteLine("{0} - {1} ${2}", @event.City, @event.Name, @event.Price);
    }

    private static int GetCachedDistance(string fromCity, string toCity)
    {

        int maxRetries = 5;
        int retries = 0;
        string key = fromCity + toCity;
        int distance = int.MinValue;

        Stopwatch lookupTimer = new Stopwatch();
        lookupTimer.Start();
        if (fromCity == toCity)
        {
            distance = 0;
        }
        else
        {
            if (!journeyDistance.TryGetValue(key, out distance))
            {
                do
                {
                    Console.WriteLine("\tUsing Distance Service");
                    try
                    {
                        distance = getDistance(fromCity, toCity);
                    }
                    catch (IOException ioex)
                    {
                        Console.WriteLine("\tDistance lookup failed: {0}", ioex.Message);
                        retries++;
                        distance = int.MaxValue;
                        Console.WriteLine("\tWaiting {0} seconds...", retries);
                        Thread.Sleep(1000 * retries); // Linear backoff function
                    }
                } while (retries < maxRetries && distance == int.MaxValue);


                if (distance < int.MaxValue) // Lookup did not throw an exception
                {
                    journeyDistance.Add(key, distance);
                }

            }
            else
            {
                Console.WriteLine("\tCached Distance");
            }
        }
        lookupTimer.Stop();

        Console.WriteLine("\t{0} to {1} is {2} miles", fromCity, toCity, distance);
        Console.WriteLine("\tOperation took {0} ms", lookupTimer.ElapsedMilliseconds);


        return distance;

    }

    private static int getDistance(String fromCity, String toCity)
    {

        var coordinates =
            cities.Where(c => new List<String> { fromCity, toCity }.Contains(c.Name))
            .Select(c => c.Coordinate).ToArray<Coordinate>();

        Random rnd = new Random();
        int errorOdds = rnd.Next(1, 100);

        // Randomly throw an IOException to 
        if (errorOdds > 80)
        {
            throw new IOException("Random Exception");
        }


        double distance = getActualDistance(journey: new Journey { start = coordinates[0], end = coordinates[1] });

        return (int)distance;

    }

    private static double getActualDistance(Journey journey)
    {
        return Haversine(journey.start, journey.end, DistanceUnit.Miles);
    }

    public enum DistanceUnit
    {
        Miles,
        Kilometers
    }


    /**
     * Calculate rough distance over the surface of the earth (as the crow flies, or walks in decent,
     * sensible shoes)
     */

    private static double Haversine(Coordinate pos1, Coordinate pos2, DistanceUnit unit = DistanceUnit.Miles)
    {
        double R = (unit == DistanceUnit.Miles) ? 3960 : 6371;
        var lat = (pos2.Latitude - pos1.Latitude).ToRadians();
        var lng = (pos2.Longitude - pos1.Longitude).ToRadians();
        var h1 = Math.Sin(lat / 2) * Math.Sin(lat / 2) +
                      Math.Cos(pos1.Latitude.ToRadians()) * Math.Cos(pos2.Latitude.ToRadians()) *
                      Math.Sin(lng / 2) * Math.Sin(lng / 2);
        var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));
        Random rnd = new Random();
        int waitFactor = rnd.Next(1, 2);
        Thread.Sleep(waitFactor * 1000); // simulate network call
        return R * h2;
    }

    /**
     * Generate random ticket prices
     */
    private static void GenerateEventPrices()
    {
        // Generate ticket prices once
        if (pricedEvents.Count == 0)
        {

            pricedEvents = events.Select(e => new Event { Name = e.Name, City = e.City, Price = getPrice(e).Price }).ToList();
        }
    }

    /**
     * Concerts are priced between $50 - %250
     */
    private static Event getPrice(Event @event)
    {
        Random rnd = new Random();
        int priceFactor = rnd.Next(1, 5);
        double price = 50.0 * (double)priceFactor;
        @event.Price = price;
        return @event;
    }
}