using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace LAB__2_Threading
{
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
            }

            // Check if the car crossed the finish line
            if (Distance >= 2 && Running)
            {
                lock (eventLogLock)
                {
                    Running = false;
                    eventLog.Add($"{Name} crossed the finish line!");
                }
            }
        }
    }

    class Program
    {
        private static Timer clearConsoleTimer;
        private static Sounds backgroundMusicPlayer;

        static void DisplayEvents()
        {
            while (true)
            {
                Thread.Sleep(200);

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
            Console.WriteLine("Status:");

            foreach (var car in cars)
            {
                Console.WriteLine($"{car.Name,-10}: {car.Distance:F2} km, Hastighet: {car.Speed} km/h");
            }

            // Check if any car has crossed the finish line
            if (cars.Any(car => car.Distance >= 2))
            {
                Console.WriteLine("\nRace completed!");
            }
            else
            {
                Console.WriteLine();
            }


            static async Task Main()
            {
                var car1 = new Car("Hotwheels toy car");
                var car2 = new Car("That Jagex Tank");
                var car3 = new Car("Lightning McQueen");

                var cars = new List<Car> { car1, car2, car3 };

                var threads = new List<Thread>();
                foreach (var car in cars)
                {
                    var thread = new Thread(car.SimulateRace);
                    threads.Add(thread);
                }

                //instance of music player
                backgroundMusicPlayer = new Sounds();

                // play background music asynchronously
                var backgroundMusicTask = backgroundMusicPlayer.PlaySoundAsync("tokyo.wav");

                // Start a thread to display events
                var displayEventsThread = new Thread(DisplayEvents);
                displayEventsThread.Start();

                // Set up the timer to clear the console every 60 seconds
                clearConsoleTimer = new Timer(ClearConsoleCallback, null, 0, 60000);

                foreach (var thread in threads)
                {
                    thread.Start();
                }

                // Wait for any car to finish the race
                while (cars.Any(car => car.Running))
                {
                    Thread.Sleep(1000);
                    PrintRaceStatus(cars);

                    // Check if any car has crossed the finish line
                    if (cars.Any(car => car.Distance >= 2))
                    {
                        PrintPodium(cars);
                        break;
                    }
                }

                // Stop the display events thread
                displayEventsThread.Join();

                // Find the winner
                var winner = cars.FindMax(car => car.Distance);
                Console.WriteLine($"\n{winner.Name} vann tävlingen!");

                // Wait for all cars to finish their threads
                foreach (var thread in threads)
                {
                    thread.Join();
                }

                // Dispose the timer when done
                clearConsoleTimer.Dispose();
            }

            static void PrintPodium(List<Car> cars)
            {
                Console.Clear();
                Console.WriteLine("Podium:");

                // Display the top 3 positions on the podium
                Console.WriteLine($"1st Place: {cars[0].Name,-10}");
                Console.WriteLine($"2nd Place: {cars[1].Name,-10}");
                Console.WriteLine($"3rd Place: {cars[2].Name,-10}");

                Console.WriteLine("\nRace completed!");

                Console.WriteLine($"                           {cars[0].Name,-10}\r\n                         @-----------------------@\r\n           {cars[1].Name,-10}  |           @           |\r\n@-----------------------@|           |           |\r\n|           @           ||           |           | {cars[2].Name,-10}\r\n|           |           ||           |           |@-----------------------@\r\n|           |           ||           |           ||           @           |");
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
    }
}
