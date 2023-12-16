using LAB__2_Threading;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

class Car
{
    public string Name { get; }
    public int Speed { get; }
    public double Distance { get; private set; }
    public bool Running { get; private set; }

    internal static List<string> eventLog = new List<string>();
    internal static object eventLogLock = new object();

    public Car(string name, int speed = 120)
    {
        Name = name;
        Speed = speed;
        Distance = 0;
        Running = true;
    }

    public void SimulateRace()
    {
        while (Running && Distance < 10)
        {
            Thread.Sleep(100);  // Simulating time passing
            Distance += Speed / 3600.0;  // Converting speed to km/s

            if (new Random().NextDouble() < 1.0 / 30)
            {
                Task.Run(() => PlayEventSound("EventName")); // Start playing event sound in the background
                HandleEvent();
            }
        }
    }

    private void HandleEvent()
    {
        var events = new List<(string, double, int)>
        {
            ("Slut på bensin", 1.0 / 1200, 30000),
            ("Punktering", 2.0 / 700, 20000),
            ("Fågel på vindrutan", 5.0 / 5000, 10000),
            ("Krash", 1.0 / 50000, -1),
            ("Turbo-boost", 1.0 / 3500, 15000),
            ("Plötslig regnstorm", 3.0 / 1800, 20000),
            ("Motorfel", 10.0 / 10000, 20000),
        };

        var random = new Random();
        var eventIndex = random.Next(events.Count);
        var (eventName, _, duration) = events[eventIndex];

        Console.WriteLine($"{Name} har fått ett problem: {eventName}");

        PlayEventSound(eventName);  // Play sound for the event

        // Add the event to the event log
        lock (eventLogLock)
        {
            eventLog.Add($"{Name}: {eventName} ({duration / 1000} s)");
        }

        Thread.Sleep(duration);
        Console.WriteLine($"{Name} fortsätter tävla!");
    }

    private async void PlayEventSound(string eventName)
    {
        // Assume you have sound files named "eventName.wav" for each event
        string soundFileName = $"{eventName}.wav";
        await Sounds.PlaySoundAsync(soundFileName);
    }
}

class Program
{
    private static Timer clearConsoleTimer;

    private static async Task PlayBackgroundMusicAsync()
    {
        string backgroundMusicFileName = "tokyo.wav";
        await Sounds.PlaySoundAsync(backgroundMusicFileName);
    }

    static void DisplayEvents()
    {
        while (true)
        {
            Thread.Sleep(200);  // Adjusted delay for more responsive output

            lock (Car.eventLogLock)
            {
                Console.SetCursorPosition(Console.WindowWidth - 30, 0);
                Console.WriteLine("Event Log:");

                for (int i = 0; i < Car.eventLog.Count; i++)
                {
                    Console.SetCursorPosition(Console.WindowWidth - 30, i + 1);
                    Console.WriteLine(Car.eventLog[i]);
                }
            }
        }
    }

    static void PrintRaceStatus(List<Car> cars)
    {
        Console.Clear();
        Console.WriteLine("Tävlingsstatus:");

        foreach (var car in cars)
        {
            Console.WriteLine($"{car.Name,-10}: {car.Distance:F2} km, Hastighet: {car.Speed} km/h");
        }

        Console.WriteLine();
    }

    static async Task Main()
    {
        var car1 = new Car("Bil 1");
        var car2 = new Car("Bil 2");

        var cars = new List<Car> { car1, car2 };

        var threads = new List<Thread>();
        foreach (var car in cars)
        {
            var thread = new Thread(car.SimulateRace);
            threads.Add(thread);
        }

        // Start playing the background music asynchronously
        Task.Run(() => PlayBackgroundMusicAsync());

        // Start a thread to display events
        var displayEventsThread = new Thread(DisplayEvents);
        displayEventsThread.Start();

        // Set up the timer to clear the console every 60 seconds
        clearConsoleTimer = new Timer(ClearConsoleCallback, null, 0, 60000);

        foreach (var thread in threads)
        {
            thread.Start();
        }

        while (cars.Exists(car => car.Running))
        {
            Thread.Sleep(1000);
            PrintRaceStatus(cars);
        }

        var winner = cars.FindMax(car => car.Distance);
        Console.WriteLine($"\n{winner.Name} vann tävlingen!");

        // Stop the display events thread
        displayEventsThread.Join();

        foreach (var thread in threads)
        {
            thread.Join();
        }

        // Dispose the timer when done
        clearConsoleTimer.Dispose();
    }

    static void ClearConsoleCallback(object state)
    {
        Console.Clear();
    }
}

public static class Extensions
{
    public static T FindMax<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
        where TKey : IComparable<TKey>
    {
        using (var enumerator = source.GetEnumerator())
        {
            if (!enumerator.MoveNext())
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }

            var maxElement = enumerator.Current;
            var maxValue = selector(maxElement);

            while (enumerator.MoveNext())
            {
                var element = enumerator.Current;
                var value = selector(element);

                if (value.CompareTo(maxValue) > 0)
                {
                    maxElement = element;
                    maxValue = value;
                }
            }

            return maxElement;
        }
    }
}
