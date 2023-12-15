    namespace LAB__2_Threading
    {
        using System;
        using System.Collections.Generic;
        using System.Threading;
   

        class Car
        {
            public string Name { get; }
            public int Speed { get; }
            public double Distance { get; private set; }
            public bool Running { get; private set; }

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
                        HandleEvent();
                    }
                }
            }

            private void HandleEvent()
            {
                var events = new List<(string, double, int)>
            {
                ("Slut på bensin", 1.0 / 50, 30000),
                ("Punktering", 2.0 / 50, 20000),
                ("Fågel på vindrutan", 5.0 / 50, 10000),
                ("Krash", 1.0 / 1000, -1),
                ("Turbo-boost", 1.0 / 50, 15000),
                ("Plötslig regnstorm", 3.0 / 50, 20000),
                ("Motorfel", 10.0 / 50, 20000),
                ("Kollision", 1.0 / 500, 50000)
            };

                var random = new Random();
                var eventIndex = random.Next(events.Count);
                var (eventName, _, duration) = events[eventIndex];

                Console.WriteLine($"{Name} har fått ett problem: {eventName}");
                Thread.Sleep(duration);
                Console.WriteLine($"{Name} fortsätter tävla!");
            }
        }



    class Program
    {
        static async Task PlayBackgroundMusicAsync()
        {
            string backgroundMusicFileName = "tokyo.wav";
            await Sounds.PlaySoundAsync(backgroundMusicFileName);
        }

        static async Task Main()
        {
            var car1 = new Car("Bil 1");
            var car2 = new Car("Bil 2");
            var car3 = new Car("Bil 3");
            var car4 = new Car("Bil 4");
            var car5 = new Car("Bil 5");

            var cars = new List<Car> { car1, car2, car3, car4, car5 };

            var threads = new List<Thread>();
            foreach (var car in cars)
            {
                var thread = new Thread(car.SimulateRace);
                threads.Add(thread);
            }
            static void PrintRaceStatus(List<Car> cars)
            {
                Console.WriteLine("\nTävlingsstatus:");
                foreach (var car in cars)
                {
                    Console.WriteLine($"{car.Name}: {car.Distance:F2} km, Hastighet: {car.Speed} km/h");
                }
                Console.WriteLine();
            }

            // Start playing the background music asynchronously, so we can play music without affecting the program negatively
            PlayBackgroundMusicAsync();

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

            foreach (var thread in threads)
            {
                thread.Join();
            }
        }
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