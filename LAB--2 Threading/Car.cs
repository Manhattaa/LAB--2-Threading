using System;
using System.Collections.Generic;
using System.Threading;

namespace LAB__2_Threading
{
    class Car
    {
        const double raceDistance = 10.0;
        public string Name { get; }
        public int Speed { get; set; }
        public double Distance { get; private set; }
        public bool Running { get; set; }
        public DateTime LastEventTime { get; set; }

        internal static List<string> eventLog = new List<string>();
        internal static object eventLogLock = new object();
        internal static Random random = new Random();

        private object randomEventLock = new object(); // Move to instance level

        public Car(string name, Random random, int speed = 120)
        {
            Name = name;
            Speed = speed;
            Distance = 0;
            Running = true;
            LastEventTime = DateTime.Now;
            RandomInstance = random ?? new Random();
        }

        public Random RandomInstance { get; } // Property to hold the Random instance

        public void SimulateRace()
        {
            while (Running && Distance < raceDistance)
            {
                Thread.Sleep(100);
                Distance += Speed / 3600.0;
                HandleRandomEvent();
            }

            if (Distance >= raceDistance && Running)
            {
                lock (eventLogLock)
                {
                    Running = false;
                    eventLog.Add($"{Name} crossed the finish line!");
                }
            }
        }

        internal void HandleRandomEvent()
        {
            // Check if the race is still running
            if (!Running)
                return;

            // Check if enough time has passed since the last event (12 seconds)
            if ((DateTime.Now - LastEventTime).TotalSeconds < 12)
                return;

            lock (randomEventLock)
            {
                // Generate a random event for a random car
                int eventType = RandomInstance.Next(4); // Use the car's specific Random instance

                double distanceChange = 0; // Variable to store the distance change

                switch (eventType)
                {
                    case 0: // you're outta soup - 1/50
                        lock (eventLogLock)
                        {
                            distanceChange = -15.0; // Simulate stopping for 15 seconds
                            eventLog.Add($"{Name}: You run out of gas. (15 s)");
                        }
                        break;

                    case 1: // Tire puncture - 2/50
                        lock (eventLogLock)
                        {
                            distanceChange = -20.0; // Simulate stopping for 20 seconds
                            eventLog.Add($"{Name}: The tires get punctured. (20 s)");
                        }
                        break;

                    case 2: // Bird hits windshield - 5/50 probability
                        lock (eventLogLock)
                        {
                            distanceChange = -5.0; // Simulate stopping for 5 seconds
                            eventLog.Add($"{Name}: Bird hits the windshield (5 s)");
                        }
                        break;

                    case 3: // Engine Failure! - 10/50 probability
                        lock (eventLogLock)
                        {
                            Speed -= 1; // Simulate engine failure by reducing speed
                            eventLog.Add($"{Name} experienced engine failure. (1 km/h slower)");
                        }
                        break;
                }

                // Update the distance only if it doesn't make the distance negative
                if (Distance + distanceChange >= 0)
                {
                    Distance += distanceChange;
                }

                // Update the LastEventTime for the car
                LastEventTime = DateTime.Now;
            }
        }
    }
}
