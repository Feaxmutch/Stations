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

        public static bool TryGetNumberFromUser(string massage, out int parsedNumber, bool canBeNegative = true)
        {
            Console.Write(massage);
            string userInput = Console.ReadLine();

            if (userInput != string.Empty)
            {
                if (int.TryParse(userInput, out int number))
                {
                    if (int.IsNegative(number) == false || canBeNegative)
                    {
                        parsedNumber = number;
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Отрицательное число не допустимо.");
                    }
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

    class PeoplesFactory
    {
        private int _minPeoples;
        private int _maxPeoples;

        public PeoplesFactory(int minPeoples, int maxPeoples)
        {
            _minPeoples = minPeoples;
            _maxPeoples = maxPeoples;
        }

        public int CreatePeoples()
        {
            return Utilits.GetRandomNumber(_minPeoples, _maxPeoples);
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
                RemoveTrainsInDestination();
                ShowTrains();
                Console.WriteLine();
                MoveTrains();

                if (TryCreateTrain(out Train train))
                {
                    _trains.Add(train);
                }
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

        private void MoveTrains()
        {
            foreach (var train in _trains)
            {
                train.Move();
            }
        }

        private void RemoveTrainsInDestination()
        {
            for (int i = _trains.Count - 1; i >= 0; i--)
            {
                if (_trains[i].InDestination)
                {
                    _trains.RemoveAt(i);
                }
            }
        }

        private void ShowStations()
        {
            for (int i = 0; i < _stations.Count; i++)
            {
                Console.WriteLine($"{i + 1}. \"{_stations[i].Name}\" позиция {_stations[i].Position}");
            }
        }

        private bool TryCreateTrain(out Train train)
        {
            train = default;
            bool isPatchCreated = TryCreatePath(out Path path);
            int passengers = new PeoplesFactory(10, 500).CreatePeoples();

            if (isPatchCreated)
            {
                List<Wagon> wagons = BoardPassengers(passengers);
                train = new(path, wagons);
                Console.WriteLine($"Поезд и маршрут для него созданы. На данный маршрут было продано {passengers} билетов.");
            }
            else
            {
                Console.WriteLine("Поезд не создан.");
            }

            Console.ReadKey();
            return isPatchCreated;
        }

        private bool TryCreatePath(out Path path)
        {
            path = default;
            bool isStationsGeted = default;
            bool isSuccessfull = default;

            Console.WriteLine("Станции:");
            ShowStations();
            isStationsGeted = TryGetStationByNumber(out Station startStation);
            isStationsGeted = TryGetStationByNumber(out Station endStation) && isStationsGeted;

            if (isStationsGeted)
            {
                if (startStation != endStation)
                {
                    path = new(startStation, endStation);
                    isSuccessfull = true;
                }
                else
                {
                    Console.WriteLine("Маршрут не допустим.\n" +
                                      "Точка отправления и прибытия одинаковые.");
                }
            }

            return isSuccessfull;
        }

        private List<Wagon> BoardPassengers(int passengers)
        {
            int capacity = default;
            bool isCorrectCapacity = default;
            List<Wagon> wagons = new();

            while (isCorrectCapacity == false)
            {
                if (Utilits.TryGetNumberFromUser("Введите вместимость вагона: ", out capacity, false))
                {
                    if (capacity == 0)
                    {
                        Console.WriteLine("Вместимость вагона не может быть равна 0");
                    }

                    isCorrectCapacity = capacity > 0;
                }
            }

            while (passengers > 0)
            {
                int enteredPassengers = Math.Min(capacity, passengers);
                Wagon nextWagon = new(capacity);
                nextWagon.AddPeoples(enteredPassengers);
                wagons.Add(nextWagon);
                passengers -= enteredPassengers;
            }

            return wagons;
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

        public bool InDestination => Position == Path.End.Position;

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
