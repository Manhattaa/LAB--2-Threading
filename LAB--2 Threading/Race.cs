using LAB__2_Threading.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LAB__2_Threading.Utilities;

namespace LAB__2_Threading
{
        class Race
        {
            private static Timer clearConsoleTimer;
            private static Sounds backgroundMusicPlayer;
            private static Random random = new Random();
            private static bool raceCompleted = false;
            private static Timer randomEventTimer;

        public static void DisplayEvents()
            {
                while (!raceCompleted)
                {
                    Thread.Sleep(200);

                    lock (Car.eventLogLock)
                    {
                        Console.SetCursorPosition(Console.WindowWidth - 60, 0);
                        Console.WriteLine("Event Log:");

                        for (int i = 0; i < Car.eventLog.Count; i++)
                        {
                            Console.SetCursorPosition(Console.WindowWidth - 60, i + 1);
                            Console.WriteLine(Car.eventLog[i]);
                        }
                    }
                }
            }

            public static void PrintRaceStatus(List<Car> cars)
            {
                Console.Clear();
                Console.WriteLine("Status:");

                foreach (var car in cars)
                {
                    Console.WriteLine($"{car.Name,-10}: {car.Distance:F2} km, Speed: {car.Speed} km/h");
                }

                if (cars.Any(car => car.Distance >= 10))
                {
                    Console.WriteLine("\nRace completed!");
                }
                else
                {
                    Console.WriteLine();
                }
            }

            public static async Task RunAsync()
            {
                var car1 = new Car("Mystery Machine", random: random);
                var car2 = new Car("Vroom Vroom", random: random);
                var car3 = new Car("Christoffer", random: random);
                var car4 = new Car("Gamla Bettan", random: random);
                var car5 = new Car("Lightning McQueen", random: random);
                var car6 = new Car("Aldor", random: random);

                car1.LastEventTime = DateTime.Now;
                car2.LastEventTime = DateTime.Now;
                car3.LastEventTime = DateTime.Now;

                var cars = new List<Car> { car1, car2, car3, car4, car5, car6 };

                var threads = new List<Thread>();
                foreach (var car in cars)
                {
                    var thread = new Thread(car.SimulateRace);
                    threads.Add(thread);
                }

                backgroundMusicPlayer = new Sounds();
                var backgroundMusicTask = Sounds.PlaySoundAsync("tokyo.wav");

                var displayEventsThread = new Thread(DisplayEvents);
                displayEventsThread.Start();

                clearConsoleTimer = new Timer(ClearConsoleCallback, null, 0, 60000);

                var randomEventTimer = new Timer(GenerateRandomEventCallback, cars, 0, 12000);

                foreach (var thread in threads)
                {
                    thread.Start();
                }

                while (cars.Any(car => car.Distance < 10))
                {
                    Thread.Sleep(1000);
                    PrintRaceStatus(cars);

                    if (cars.Any(car => car.Distance >= 10))
                    {
                        PrintPodium(cars);
                        raceCompleted = true;
                        break;
                    }
                }

                displayEventsThread.Join();
                randomEventTimer.Change(Timeout.Infinite, Timeout.Infinite);

                var winner = cars.FindMax(car => car.Distance);
                Console.WriteLine($"\n{winner.Name} vann tävlingen!");

                foreach (var car in cars)
                {
                    car.Running = false;
                }

                foreach (var thread in threads)
                {
                    thread.Join();
                }

                clearConsoleTimer.Dispose();
                randomEventTimer.Dispose();
            }

        private static void GenerateRandomEventCallback(object state)
        {
            var cars = (List<Car>)state;

            // Choose a random car to generate an event
            var randomCar = cars[random.Next(cars.Count)];

            // Generate a random event for the selected car
            randomCar.HandleRandomEvent();
        }

        public static void PrintPodium(List<Car> cars)
        {
            Console.Clear();
            Console.WriteLine("Podium:");

            // Sort the cars based on the distance covered in descending order
            var sortedCars = cars.OrderByDescending(car => car.Distance).ToList();

            Console.WriteLine($"1st Place: {sortedCars[0].Name,-10}");
            Console.WriteLine($"2nd Place: {sortedCars[1].Name,-10}");
            Console.WriteLine($"3rd Place: {sortedCars[2].Name,-10}");

            Console.Clear();
            Console.WriteLine("\nRace completed!");
            Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n");
            Console.WriteLine($"                           {sortedCars[0].Name,-10}\r\n                         @-----------------------@\r\n       {sortedCars[1].Name,-10}  |           @           |\r\n@-----------------------@|           |           |\r\n|           @           ||           |           | {sortedCars[2].Name,-10}\r\n|           |           ||           |           |@-----------------------@\r\n|           |           ||           |           ||           @           |");
        }

        private static void ClearConsoleCallback(object state)
            {
                if (!raceCompleted)
                {
                    Console.Clear();
                }
            }
        }
    }
