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
            Console.WriteLine("Select mode:");
            Console.WriteLine("1. Tokyo Mode");
            Console.WriteLine("2. 12138127a84483e726b06ebf8397d64d Mode");

            int modeChoice;
            while (!int.TryParse(Console.ReadLine(), out modeChoice) || (modeChoice != 1 && modeChoice != 2))
            {
                Console.WriteLine("Invalid choice. Please enter 1 or 2.");
            }

            string backgroundMusic = (modeChoice == 1) ? "tokyo.wav" : "yeltsa kcir.wav";

            var car1 = new Car((modeChoice == 2) ? "Rick" : "Mystery Machine", random: random);
            var car2 = new Car((modeChoice == 2) ? "Rick" : "Vroom Vroom", random: random);
            var car3 = new Car((modeChoice == 2) ? "Rick" : "Christoffer", random: random);
            var car4 = new Car((modeChoice == 2) ? "Rick" : "Gamla Bettan", random: random);
            var car5 = new Car((modeChoice == 2) ? "Rick" : "Lightning McQueen", random: random);
            var car6 = new Car((modeChoice == 2) ? "Rick" : "Aldor ", random: random);

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
            var backgroundMusicTask = Sounds.PlaySoundAsync(backgroundMusic);

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
            Console.WriteLine($"\n{winner.Name} is the Winner of this competition!");

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
                Console.WriteLine($"4th Place: {sortedCars[3].Name,-10}");
                Console.WriteLine($"5th Place: {sortedCars[4].Name,-10}");
                Console.WriteLine($"6th Place: {sortedCars[5].Name,-10}");


                Console.Write("Honorable Mentions:");
                Console.Write($"{sortedCars[3].Name,-10} :");
                Console.Write($"{sortedCars[4].Name,-10} :");
                Console.Write($"{sortedCars[5].Name,-10} :");


                Console.Clear();
                Console.WriteLine("\nRace completed!");
                Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\n\n");
                Console.WriteLine($"                          {sortedCars[0].Name,-10}\r\n                         @-----------------------@\r\n       {sortedCars[1].Name,-10}        |           @           |\r\n@-----------------------@|           |           |\r\n|           @           ||           |           | {sortedCars[2].Name,-10}\r\n|           |           ||           |           |@-----------------------@\r\n|           |           ||           |           ||           @           |");
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
