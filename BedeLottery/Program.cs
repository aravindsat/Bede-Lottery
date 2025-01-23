using System;
using System.Collections.Generic;
using System.Linq;

namespace LotteryGame
{
    class Program
    {
        static void Main(string[] args)
        {
            //LotteryConfig config = new LotteryConfig
            //{
            //    PlayerBalance = GetValidInput("Enter the starting balance for each player ($10 default): ", 10, 1, 100),
            //    TicketPrice = GetValidInput("Enter the ticket price ($1 default): ", 1, 1, 10),
            //    MinPlayers = GetValidInput("Enter the minimum number of players (10 default): ", 10, 2, 50),
            //    MaxPlayers = GetValidInput("Enter the maximum number of players (15 default): ", 15, 10, 100)
            //};
            LotteryConfig config = new LotteryConfig
            {
                PlayerBalance = 10, // Hardcoded value for player balance
                TicketPrice = 1,    // Hardcoded value for ticket price
                MinPlayers = 10,    // Hardcoded value for minimum players
                MaxPlayers = 15     // Hardcoded value for maximum players
            };

            LotteryManager lotteryManager = new LotteryManager(config);
            lotteryManager.RunLottery();
        }

       public static int GetValidInput(string prompt, int defaultValue, int minValue, int maxValue)
        {
            Console.Write(prompt);
            string input = Console.ReadLine();

            if (int.TryParse(input, out int value) && value >= minValue && value <= maxValue)
            {
                return value;
            }
            else
            {
                Console.WriteLine($"Invalid input. Using default value: {defaultValue}\n");
                return defaultValue;
            }
        }
    }

    class LotteryManager
    {
        private readonly LotteryConfig _config;
        private readonly Random _random;
        private List<Player> _players;

        public LotteryManager(LotteryConfig config)
        {
            _config = config;
            _random = new Random();
            _players = new List<Player>();
        }

        public void RunLottery()
        {
            InitializePlayers();
            SellTickets();
            DisplayResults(CalculateWinners());

            // Pause to allow the user to see the results
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private void InitializePlayers()
        {
            Console.WriteLine("\nWelcome to the Bede Lottery, Player 1!\n");
            Console.WriteLine($"* Your digital balance: ${_config.PlayerBalance:0.00}\n * Ticket Price: ${_config.TicketPrice:0.00} each\n");

            _players.Add(new Player("Player 1", _config.PlayerBalance));

            int totalPlayers = _random.Next(_config.MinPlayers, _config.MaxPlayers + 1);
            for (int i = 2; i <= totalPlayers; i++)
            {
                _players.Add(new Player($"Player {i}", _config.PlayerBalance));
            }

            Console.WriteLine($"{_players.Count - 1} other players have joined the lottery.\n");
        }

        private void SellTickets()
        {
            Console.Write("How many tickets do you want to buy, Player 1? \n");
            int player1Tickets = Program.GetValidInput("", 0, 0, _config.PlayerBalance / _config.TicketPrice);
            _players[0].BuyTickets(player1Tickets, _config.TicketPrice);

            foreach (var player in _players.Skip(1))
            {
                int ticketsRequested = _random.Next(1, 11);
                player.BuyTickets(ticketsRequested, _config.TicketPrice);
            }

            Console.WriteLine($"\nTickets purchased by all players.\n");
        }

        private (Player grandPrizeWinner, List<Player> secondTierWinners, List<Player> thirdTierWinners, decimal houseProfit) CalculateWinners()
        {
            List<(Player player, int ticketNumber)> ticketsPool = _players
                .SelectMany(p => Enumerable.Range(1, p.Tickets).Select(ticket => (p, ticket))).ToList();

            ticketsPool = ticketsPool.OrderBy(x => _random.Next()).ToList();

            int totalTickets = ticketsPool.Count;
            int totalRevenue = totalTickets * _config.TicketPrice;

            int grandPrize = (int)(totalRevenue * 0.50);
            int secondTierWinnersCount = (int)Math.Round(totalTickets * 0.10);
            int thirdTierWinnersCount = (int)Math.Round(totalTickets * 0.20);

            decimal secondTierPrize = secondTierWinnersCount > 0 ? Math.Round((decimal)(totalRevenue * 0.30) / secondTierWinnersCount, 2) : 0;
            decimal thirdTierPrize = thirdTierWinnersCount > 0 ? Math.Round((decimal)(totalRevenue * 0.10) / thirdTierWinnersCount, 2) : 0;

            var grandPrizeWinner = ticketsPool[0].player;
            ticketsPool.RemoveAt(0);

            var secondTierWinners = ticketsPool.Take(secondTierWinnersCount).Select(t => t.player).Distinct().ToList();
            ticketsPool = ticketsPool.Skip(secondTierWinnersCount).ToList();

            var thirdTierWinners = ticketsPool.Take(thirdTierWinnersCount).Select(t => t.player).Distinct().ToList();

            decimal houseProfit = totalRevenue - (grandPrize + (secondTierPrize * secondTierWinners.Count) + (thirdTierPrize * thirdTierWinners.Count));

            return (grandPrizeWinner, secondTierWinners, thirdTierWinners, houseProfit);
        }

        private void DisplayResults((Player grandPrizeWinner, List<Player> secondTierWinners, List<Player> thirdTierWinners, decimal houseProfit) results)
        {
            Console.WriteLine("Ticket Draw Results: \n");

            Console.WriteLine($"* Grand Prize: {results.grandPrizeWinner.Name} wins ${_config.PlayerBalance * 0.50:0.00}!");

            if (results.secondTierWinners.Any())
            {
                Console.WriteLine("* Second Tier: Players " + string.Join(", ", results.secondTierWinners.Select(p => p.Name)) + $" win ${_config.PlayerBalance * 0.30} each!");
            }

            if (results.thirdTierWinners.Any())
            {
                Console.WriteLine("* Third Tier: Players " + string.Join(", ", results.thirdTierWinners.Select(p => p.Name)) + $" win ${_config.PlayerBalance * 0.10} each!");
            }

            Console.WriteLine();
            Console.WriteLine("Congratulations to the winners!\n");
            Console.WriteLine($"House Revenue: ${results.houseProfit:0.00}\n");
        }
    }

    class Player
    {
        public string Name { get; set; }
        public int Balance { get; set; }
        public int Tickets { get; private set; }

        public Player(string name, int balance)
        {
            Name = name;
            Balance = balance;
            Tickets = 0;
        }

        public int BuyTickets(int requestedTickets, int ticketPrice)
        {
            int affordableTickets = Math.Min(requestedTickets, Balance / ticketPrice);
            Tickets += affordableTickets;
            Balance -= affordableTickets * ticketPrice;
            return affordableTickets;
        }
    }

    class LotteryConfig
    {
        public int PlayerBalance { get; set; }
        public int TicketPrice { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
    }
}
