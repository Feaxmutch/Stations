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
                    isWorking = false;
                    continue;
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
                distance = Math.Abs(train.Path.End.Position - train.Position);
                peoplesCount = train.CalculatePeoples();
                startName = train.Path.Start.Name;
                endName = train.Path.End.Name;
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
            bool stationsIsGeted = default;
            int peoples = PeoplesGenerator.GeneratePeoples();
            int passengers = peoples;

            Console.WriteLine("Станции:");
            ShowStations();
            stationsIsGeted = TryGetStationByNumber(out Station startStation);
            stationsIsGeted = TryGetStationByNumber(out Station endStation) && stationsIsGeted;

            if (stationsIsGeted)
            {
                if (startStation != endStation)
                {
                    List<Wagon> wagons = new();

                    if (Utilits.TryGetNumberFromUser("Введите вместимость вагона: ", out int capacity))
                    {
                        while (passengers > 0)
                        {
                            int enteredPassengers = Math.Min(capacity, passengers);
                            Wagon nextWagon = new(enteredPassengers);
                            nextWagon.AddPeoples(capacity);
                            wagons.Add(nextWagon);
                            passengers -= enteredPassengers;
                        }
                    }

                    Path path = new(startStation, endStation);
                    Train newTrain = new(path, wagons);
                    _trains.Add(newTrain);
                    isComplete = true;
                }
                else
                {
                    Console.WriteLine("Маршрут не допустим.\n" +
                                      "Точка отправления и прибытия одинаковые.");
                }
            }

            if (isComplete)
            {
                Console.WriteLine($"Поезд и маршрут для него созданы. На данный маршрут было продано {peoples} билетов.");
            }
            else
            {
                Console.WriteLine("Поезд не создан.");
            }

            Console.ReadKey();
        }

        private bool TryGetStationByNumber(out Station station)
        {
            if (Utilits.TryGetNumberFromUser("Введите номер станции: ", out int number))
            {
                if (HaveStationNumber(number))
                {
                    station = _stations[number - 1];
                    return true;
                }
                else
                {
                    Console.WriteLine($"Номера {number} нет в списке станций");
                }
            }

            station = new();
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
                    if (_stations[i] == _stations[j])
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

        public static bool operator ==(Station station1, Station station2)
        {
            bool equalsName = station1.Name == station2.Name;
            bool equalsPosition = station1.Position == station2.Position;
            return equalsName && equalsPosition;
        }

        public static bool operator !=(Station station1, Station station2)
        {
            return (station1 == station2) == false;
        }
    }

    class Path
    {
        public Path(Station start, Station end)
        {
            Start = start;
            End = end;
        }

        public Station Start { get; }

        public Station End { get; }
    }

    class Train
    {
        private List<Wagon> _wagons = new();

        public Train(Path path, List<Wagon> wagons)
        {
            _wagons = new(wagons);
            Path = path;
            Position = path.Start.Position;
        }

        public int Position { get; private set; }

        public Path Path { get; }

        public bool InDestination  => Position == Path.End.Position;

        public void Move()
        {
            if (Position < Path.End.Position)
            {
                Position++;
            }
            else if (Position > Path.End.Position)
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

        public void AddPeoples(int quantity)
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
