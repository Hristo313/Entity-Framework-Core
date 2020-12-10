namespace BookShop
{
    using BookShop.Models;
    using Data;
    using Initializer;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Z.EntityFramework.Plus;

    public class StartUp
    {
        public static StringBuilder sb = new StringBuilder();

        public static void Main()
        {
            using var db = new BookShopContext();
            //DbInitializer.ResetDatabase(db);

            //int input = int.Parse(Console.ReadLine());
            int result = RemoveBooks(db);
            Console.WriteLine(result);

            IncreasePrices(db);
        }

        //Exercise 2.Age Restriction
        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            var bookTitles = context
                .Books
                .ToList()
                .Where(b => b.AgeRestriction.ToString().ToLower() == command.ToLower())
                .Select(b => b.Title)
                .OrderBy(b => b)
                .ToList();

            foreach (var title in bookTitles)
            {
                sb.AppendLine($"{title}");
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 3.Golden Books
        public static string GetGoldenBooks(BookShopContext context)
        {
            var bookTitles = context
                .Books
                .ToList()
                .Where(b => b.EditionType.ToString() == "Gold" && b.Copies < 5000)
                .Select(b => new { b.Title, b.BookId })
                .OrderBy(b => b.BookId)
                .ToList();

            foreach (var bt in bookTitles)
            {
                sb.AppendLine($"{bt.Title}");
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 4.Books by Price
        public static string GetBooksByPrice(BookShopContext context)
        {
            var bookTitles = context
                .Books
                .ToList()
                .Where(b => b.Price > 40)
                .Select(b => new { b.Title, b.Price })
                .OrderByDescending(b => b.Price)
                .ToList();

            foreach (var bt in bookTitles)
            {
                sb.AppendLine($"{bt.Title} - ${bt.Price:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 5.Not Released In
        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            var bookTitles = context
                .Books
                .Where(b => b.ReleaseDate.Value.Year != year)
                .Select(b => new { b.BookId, b.Title })
                .OrderBy(b => b.BookId)
                .ToList();

            foreach (var bt in bookTitles)
            {
                sb.AppendLine($"{bt.Title}");
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 6.Book Titles by Category
        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            string[] categories = input
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.ToLower())
                .ToArray();

            List<string> bookTitles = new List<string>();

            foreach (string category in categories)
            {
               List<string> categoryBookTitles = context
                .Books
                .Where(b => b.BookCategories.Any(bc => bc.Category.Name.ToLower() == category))
                .Select(b => b.Title)
                .ToList();

                bookTitles.AddRange(categoryBookTitles);
            }

            bookTitles = bookTitles.OrderBy(b => b).ToList();

            foreach (string bt in bookTitles)
            {
                sb.AppendLine($"{bt}");
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 7.Released Before Date
        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            var currentDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var bookTitles = context
                .Books
                .ToList()
                .Where(b => b.ReleaseDate.Value < currentDate)
                .Select(b => new
                {
                    b.Title,
                    b.EditionType,
                    b.Price,
                    b.ReleaseDate
                })               
                .OrderByDescending(b => b.ReleaseDate)
                .ToList();

            foreach (var bt in bookTitles)
            {
                sb.AppendLine($"{bt.Title} - {bt.EditionType} - ${bt.Price:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 8.Author Search
        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var authors = context
                .Authors
                .Where(a => a.FirstName.EndsWith(input))
                .Select(a => new { a.FirstName, a.LastName})
                .OrderBy(a => a.FirstName)
                .ToList();

            foreach (var a in authors)
            {
                sb.AppendLine($"{a.FirstName} {a.LastName}");
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 9.Book Search
        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            var bookTitles = context
                .Books
                .Where(b => b.Title.Contains(input))
                .Select(b => b.Title)
                .OrderBy(b => b)
                .ToList();

            return string.Join(Environment.NewLine, bookTitles);
        }

        //Exercise 10.Book Search by Author
        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var bookTitles = context
                .Books
                .Where(b => b.Author.LastName.StartsWith(input))
                .Select(b => new
                {
                    AuthorFirstName = b.Author.FirstName,
                    AuthorLastName = b.Author.LastName,
                    b.Title,
                    b.BookId
                })
                .OrderBy(b => b.BookId)
                .ToList();

            foreach (var bt in bookTitles)
            {
                sb.AppendLine($"{bt.Title} ({bt.AuthorFirstName} {bt.AuthorLastName})");
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 11.Count Books
        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            var bookTitles = context
                .Books
                .Where(b => b.Title.Length > lengthCheck)
                .ToList();

            int countBooks = bookTitles.Count();

            return countBooks;
        }

        //Exercise 12.Total Book Copies
        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var authorCopies = context
                .Authors
                .Select(a => new
                {
                    FullName = a.FirstName + " " + a.LastName,
                    BookCopies = a.Books.Sum(b => b.Copies)
                })
                .OrderByDescending(b => b.BookCopies)
                .ToList();

            foreach (var ac in authorCopies)
            {
                sb.AppendLine($"{ac.FullName} - {ac.BookCopies}");
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 13.Profit by Category
        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var categoryProfits = context
                .Categories
                .Select(c => new
                {
                    c.Name,
                    TotalProfit = c.CategoryBooks
                                    .Select(cb => new
                                    {
                                        BookProfit = cb.Book.Copies * cb.Book.Price
                                    })
                                    .Sum(cb => cb.BookProfit)
                })
                .OrderByDescending(c => c.TotalProfit)
                .ThenBy(c => c.Name)
                .ToList();

            foreach (var cp in categoryProfits)
            {
                sb.AppendLine($"{cp.Name} - ${cp.TotalProfit}");
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 14.Most Recent Books
        public static string GetMostRecentBooks(BookShopContext context)
        {
            var categoriesBooks = context
                .Categories
                .Select(c => new
                {
                    CategoryName = c.Name,
                    MostRecentBooks = c.CategoryBooks
                                       .OrderByDescending(cb => cb.Book.ReleaseDate)
                                       .Take(3)
                                       .Select(cb => new
                                       {
                                           BookTitle = cb.Book.Title,
                                           ReleaseYear = cb.Book.ReleaseDate.Value.Year
                                       })
                                       .ToList()
                })
                .OrderBy(c => c.CategoryName)
                .ToList();

            foreach (var cb in categoriesBooks)
            {
                sb.AppendLine($"--{cb.CategoryName}");

                foreach (var b in cb.MostRecentBooks)
                {
                    sb.AppendLine($"{b.BookTitle} ({b.ReleaseYear})");
                }
            }

            return sb.ToString().TrimEnd();
        }

        //Exercise 15.Increase Prices
        public static void IncreasePrices(BookShopContext context)
        {
            var books = context
               .Books
               .Where(b => b.ReleaseDate.Value.Year < 2010)
               .Update(b => new Book() { Price = b.Price + 5 });

            //foreach (var book in books)
            //{
            //    book.Price += 5;
            //}

            //context.SaveChanges();
        }

        //Exercise 16.Remove Books
        public static int RemoveBooks(BookShopContext context)
        {
            var booksToDelete = context
                .Books
                .Where(c => c.Copies < 4200)
                .Delete();

            //context.Books.RemoveRange(books);
            //int deletedBooks = context.SaveChanges();

            return booksToDelete;
        }
    }
}
