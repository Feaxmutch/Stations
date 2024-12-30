namespace stations
{
    internal class Program
    {
        static void Main()
        {
            List<Station> stations = new()
            {
                new Station("Малинкино", 1),
                new Station("житейская", 2),
                new Station("карамовая", 3),
                new Station("рыбкино", 4),
                new Station("часовская", 5),
                new Station("челябенская", 6),
            };

            Configurator configurator = new(stations);
            configurator.Work();
        }
    }

    static class Utilits
    {
        private static Random s_random = new Random();

        public static bool TryGetNumberFromUser(string massage, out int parsedNumber)
        {
            Console.Write(massage);
            string userInput = Console.ReadLine();

            if (userInput != string.Empty)
            {
                if (int.TryParse(userInput, out int number))
                {
                    parsedNumber = number;
                    return true;
                }
                else
                {
                    Console.WriteLine("Пожалуйста вводите только цифры.");
                }
            }
            else
            {
                Console.WriteLine("Вы ничего не ввели.");
            }

            parsedNumber = 0;
            return false;
        }

        public static int GetRandomNumber(int minValue, int maxValue)
        {
            return s_random.Next(minValue, maxValue);
        }
    }

    static class PeoplesGenerator
    {
        private static int MinPeoples => 10;

        private static int MaxPeoples => 500;

        public static int GeneratePeoples()
        {
            return Utilits.GetRandomNumber(MinPeoples, MaxPeoples);
        }
    }

    class Configurator
    {
        private List<Station> _stations = new();
        private List<Train> _trains = new();

        public Configurator(List<Station> stations)
        {
            foreach (var station in stations)
            {
                _stations.Add(station);
            }

            RemoveSimilarStations();
        }

        public void Work()
        {
            ConsoleKey escapeKey = ConsoleKey.Escape;
            bool isWorking = true;

            while (isWorking)
            {
                Console.WriteLine($"Нажмите {escapeKey} если хотите закрыть приложение, или любую клавишу для продолжения");

                if (Console.ReadKey().Key == escapeKey)
                {
                    return;
                }

                Console.Clear();

                for (int i = _trains.Count - 1; i >= 0; i--)
                {
                    if (_trains[i].InDestination)
                    {
                        _trains.RemoveAt(i);
                    }
                }

                ShowTrains();
                Console.WriteLine();

                foreach (var train in _trains)
                {
                    train.Move();
                }

                AddTrain();
            }
        }

        private void ShowTrains()
        {
            int distance;
            int peoplesCount;
            string startName;
            string endName;

            Console.WriteLine("Активные Маршруты:");

            foreach (var train in _trains)
            {
                Console.WriteLine();
                distance = Math.Abs(train.Destination.Position - train.Position);
                peoplesCount = train.CalculatePeoples();
                startName = train.StartStation.Name;
                endName = train.Destination.Name;
                Console.WriteLine($"От станции \"{startName}\" до станции \"{endName}\" осталось {distance} шагов. Едет {peoplesCount} пассажиров");
                train.ShowWagonsInfo();
            }
        }

        private void ShowStations()
        {
            for (int i = 0; i < _stations.Count; i++)
            {
                Console.WriteLine($"{i + 1}. \"{_stations[i].Name}\" позиция {_stations[i].Position}");
            }
        }

        private void AddTrain()
        {
            bool isComplete = false;
            int peoples = PeoplesGenerator.GeneratePeoples();

            Console.WriteLine("Станции:");
            ShowStations();

            if (TryGetStartNumber(out int startNumber))
            {
                if (TryGetEndNumber(startNumber, out int endNumber))
                {
                    Station start = _stations[startNumber - 1];
                    Station destination = _stations[endNumber - 1];
                    Train newTrain = new(start, destination, peoples);
                    _trains.Add(newTrain);
                    isComplete = true;
                }
            }

            if (isComplete)
            {
                Console.WriteLine($"Поезд и маршрут для него созданы. На данный маршрут было продано {peoples} билетов");
            }
            else
            {
                Console.WriteLine("Поезд не создан.");
            }

            Console.ReadKey();
        }

        private bool TryGetStartNumber(out int startNumber)
        {
            if (Utilits.TryGetNumberFromUser("Введите номер стартовой станции: ", out int number))
            {
                if (HaveStationNumber(number))
                {
                    startNumber = number;
                    return true;
                }
                else
                {
                    Console.WriteLine($"Номера {number} нет в списке станций");
                }
            }

            startNumber = 0;
            return false;
        }

        private bool TryGetEndNumber(int startNumber, out int endNumber)
        {
            if (Utilits.TryGetNumberFromUser("Введите номер конечной станции: ", out int number))
            {
                if (HaveStationNumber(number))
                {
                    if (number != startNumber)
                    {
                        endNumber = number;
                        return true;
                    }
                    else
                    {
                        Console.WriteLine($"Номер стартовой и конечной станции не должны совпадать.");
                    }
                }
                else
                {
                    Console.WriteLine($"Номера {number} нет в списке станций");
                }
            }

            endNumber = 0;
            return false;
        }

        private bool HaveStationNumber(int number)
        {
            return number > 0 && number <= _stations.Count;
        }

        private void RemoveSimilarStations()
        {
            for (int i = _stations.Count - 1; i > 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    if (_stations[i].Position == _stations[j].Position)
                    {
                        _stations.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }

    struct Station
    {
        public Station(string name, int position)
        {
            Name = name;
            Position = Math.Abs(position);
        }

        public string Name { get; }

        public int Position { get; }
    }

    class Train
    {
        private List<Wagon> _wagons = new();

        public Train(Station startStation, Station destination, int peoples)
        {
            StartStation = startStation;
            Destination = destination;
            Position = StartStation.Position;
            TakePeoples(peoples);
        }

        public int Position { get; private set; }

        public Station StartStation { get; }

        public Station Destination { get; }

        public bool InDestination  => Position == Destination.Position;

        private static int MaxCapasity => 50;

        private void TakePeoples(int quantity)
        {
            while (quantity > 0)
            {
                AddWagon(MaxCapasity);
                Wagon lastWagon = _wagons[_wagons.Count - 1];
                lastWagon.AddPeoples(ref quantity);
            }
        }

        public void Move()
        {
            if (Position < Destination.Position)
            {
                Position++;
            }
            else if (Position > Destination.Position)
            {
                Position--;
            }
        }

        public int CalculatePeoples()
        {
            int peoples = 0;

            foreach (var wagon in _wagons)
            {
                peoples += wagon.Peoples;
            }

            return peoples;
        }

        public void ShowWagonsInfo()
        {
            for (int i = 0; i < _wagons.Count; i++)
            {
                Console.WriteLine($"вагоне {i + 1}, с размером на {_wagons[i].Сapacity} человек, находится {_wagons[i].Peoples} пассажиров");
            }
        }

        private void AddWagon(int capasity)
        {
            _wagons.Add(new Wagon(capasity));
        }
    }

    class Wagon
    {
        public Wagon(int capacity)
        {
            Сapacity = capacity;
            Peoples = 0;
        }

        public int Peoples { get; private set; }

        public int Сapacity { get; }

        public int FreePlaces => Сapacity - Peoples;

        public void AddPeoples(ref int quantity)
        {
            if (quantity > FreePlaces)
            {
                quantity -= FreePlaces;
                Peoples = Сapacity;
            }
            else
            {
                Peoples += quantity;
                quantity = 0;
            }
        }
    }
}
