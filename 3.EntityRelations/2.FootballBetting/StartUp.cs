using _2.FootballBetting.Data;
using System;
using System.Linq;

namespace _2.FootballBetting
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            FootballBettingContext context = new FootballBettingContext();

            var users = context
                .Users
                .Select(u => new
                {
                    u.Username,
                    u.Email,
                    Name = u.Name == null ? "(No name)" : u.Name,
                    u.Balance
                });

            foreach (var u in users)
            {
                Console.WriteLine($"{u.Username} -> {u.Email} {u.Name} {u.Balance:F2}");
            }
        }
    }
}
