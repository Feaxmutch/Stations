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

    class Configurator
    {
        private List<Station> _stations = new();
        private List<Train> _trains = new();

        public Configurator(List<Station> stations)
        {
            foreach (var station in stations)
            {
                _stations.Add(station.Clone());
            }

            RemoveSimilarStations();
            MinPeoples = 0;
            MaxPeoples = 1080;
        }

        private int MinPeoples { get; }

        private int MaxPeoples { get; }

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
                if (train.Destination.Position > train.Position)
                {
                    distance = train.Destination.Position - train.Position;
                }
                else
                {
                    distance = train.Position - train.Destination.Position;
                }

                peoplesCount = train.GetAllPeoplesCount();
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

            Console.WriteLine("Станции:");
            ShowStations();

            if (TryGetStartNumber(out int startNumber))
            {
                if (TryGetEndNumber(startNumber, out int endNumber))
                {
                    int startPosition = _stations[startNumber - 1].Position;
                    int destinationPosition = _stations[endNumber - 1].Position;
                    string startName = _stations[startNumber - 1].Name;
                    string destinationName = _stations[endNumber - 1].Name;
                    Station startStation = new Station(startName, startPosition);
                    Station Destination = new Station(destinationName, destinationPosition);
                    Train newTrain = new(startStation, Destination);
                    newTrain.TakePeoples(Utilits.GetRandomNumber(MinPeoples, MaxPeoples));
                    _trains.Add(newTrain);
                    isComplete = true;
                }
            }

            if (isComplete)
            {
                Console.WriteLine($"Поезд и маршрут для него созданы. На данный маршрут было продано {_trains[_trains.Count - 1].GetAllPeoplesCount()} билетов");
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
                if (StationNumberExists(number))
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
                if (StationNumberExists(number))
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

        private bool StationNumberExists(int number)
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

    class Station
    {
        public Station(string name, int position)
        {
            Name = name;
            Position = Math.Abs(position);
        }

        public string Name { get; }

        public int Position { get; }

        public Station Clone()
        {
            return new Station(Name, Position);
        }
    }

    class Train
    {
        private List<Wagon> _wagons = new();

        public Train(Station startStation, Station destination)
        {
            StartStation = new Station(startStation.Name, startStation.Position);
            Destination = new Station(destination.Name, destination.Position);
            Position = StartStation.Position;
            MinCapasity = 1;
            MaxCapasity = 54;
        }

        public int Position { get; private set; }

        public Station StartStation { get; }

        public Station Destination { get; }

        public bool InDestination { get => Position == Destination.Position; }

        private int MinCapasity { get; }

        private int MaxCapasity { get; }

        public void TakePeoples(int quantity)
        {
            while (quantity > 0)
            {
                AddWagon(Utilits.GetRandomNumber(MinCapasity, MaxCapasity));
                Wagon lastWagon = _wagons[_wagons.Count - 1];
                quantity = lastWagon.AddPeoples(quantity);
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

        public int GetAllPeoplesCount()
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

        public int AddPeoples(int quantity)
        {
            if (quantity > FreePlaces)
            {
                quantity -= FreePlaces;
                Peoples = Сapacity;
                return quantity;
            }
            else
            {
                Peoples += Math.Max(0, quantity);
                return 0;
            }
        }
    }
}
