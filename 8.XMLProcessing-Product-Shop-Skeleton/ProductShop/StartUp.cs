using ProductShop.Data;
using ProductShop.Dtos.Export;
using ProductShop.Dtos.Export.UserCountDTO;
using ProductShop.Dtos.Import;
using ProductShop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ProductShop
{
    public class StartUp
    {
        private const string ResultDirectoryPath = "../../../Datasets/Results/";

        public static void Main(string[] args)
        {
            ProductShopContext context = new ProductShopContext();

            //ResetDatabase(context);

            //var inputXml = File.ReadAllText("../../../Datasets/categories-products.xml");

            string xml = GetUsersWithProducts(context);

            EnsureDirectoryExists(ResultDirectoryPath);

            File.WriteAllText(ResultDirectoryPath + "usersWithProducts.xml", xml);
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static void ResetDatabase(ProductShopContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            Console.WriteLine("Succesfully created Database!");
        }

        //Exercise 1.Import Users
        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            var serializer = new XmlSerializer(typeof(ImportUserDTO[]), new XmlRootAttribute("Users"));
            var usersResult = (ImportUserDTO[])serializer.Deserialize(new StringReader(inputXml));

            var users = usersResult
                .Select(u => new User
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age
                })
                .ToArray();

            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Length}";
        }

        //Exercise 2.Import Products
        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            var serializer = new XmlSerializer(typeof(ImportProductDTO[]), new XmlRootAttribute("Products"));
            var productsResult = (ImportProductDTO[])serializer.Deserialize(new StringReader(inputXml));

            var products = productsResult
                .Select(p => new Product
                {
                    Name = p.Name,
                    Price = p.Price,
                    SellerId = p.SellerId,
                    BuyerId = p.BuyerId
                })
                .ToArray();

            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Length}";
        }

        //Exercise 3.Import Categories
        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            var serializer = new XmlSerializer(typeof(ImportCategoryDTO[]), new XmlRootAttribute("Categories"));
            var categoryResult = (ImportCategoryDTO[])serializer.Deserialize(new StringReader(inputXml));

            var categories = categoryResult
                .Where(c => c.Name != null)
                .Select(c => new Category
                {
                    Name = c.Name
                })
                .ToArray();

            context.Categories.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Length}";
        }

        //Exercise 4.Import Categories and Products
        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            var serializer = new XmlSerializer(typeof(ImportCategoryProductDTO[]), new XmlRootAttribute("CategoryProducts"));
            var categoryProductsResult = (ImportCategoryProductDTO[])serializer.Deserialize(new StringReader(inputXml));

            var categoryProducts = categoryProductsResult
                .Where(cp => context.Categories.Any(c => c.Id == cp.CategoryId) &&
                             context.Products.Any(p => p.Id == cp.ProductId))
                .Select(cp => new CategoryProduct
                {
                    CategoryId = cp.CategoryId,
                    ProductId = cp.ProductId
                })
                .ToArray();

            context.CategoryProducts.AddRange(categoryProducts);
            context.SaveChanges();

            return $"Successfully imported {categoryProducts.Length}";
        }

        //Exercise 5.Products In Range
        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context
                .Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .Select(p => new ExportProductInfoDTO
                {
                    Name = p.Name,
                    Price = p.Price,
                    Buyer = p.Buyer.FirstName + " " + p.Buyer.LastName
                })
                .OrderBy(p => p.Price)
                .Take(10)
                .ToArray();

            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(ExportProductInfoDTO[]), new XmlRootAttribute("Products"));
            serializer.Serialize(new StringWriter(sb), products);

            return sb.ToString().TrimEnd();
        }

        //Exercise 6.Sold Products
        public static string GetSoldProducts(ProductShopContext context)
        {
            var users = context
                .Users
                .Where(u => u.ProductsSold.Any())
                .Select(u => new ExportUserSoldProductDTO
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    SoldProducts = u.ProductsSold.Select(sp => new ExportUserProductDTO
                    {
                        Name = sp.Name,
                        Price = sp.Price
                    })
                    .ToArray()
                })
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Take(5)
                .ToArray();

            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(ExportUserSoldProductDTO[]), new XmlRootAttribute("Users"));
            serializer.Serialize(new StringWriter(sb), users);

            return sb.ToString().TrimEnd();
        }

        //Exercise 7.Categories By Products Count
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context
                .Categories
                .Select(c => new ExportCategoryDTO
                {
                    Name = c.Name,
                    Count = c.CategoryProducts.Count,
                    AveragePrice = c.CategoryProducts.Average(cp => cp.Product.Price).ToString("F2"),
                    TotalRevenue = c.CategoryProducts.Sum(cp => cp.Product.Price).ToString("F2")
                })
                .OrderByDescending(c => c.Count)
                .ThenBy(c => c.TotalRevenue)
                .ToArray();

            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(ExportCategoryDTO[]), new XmlRootAttribute("Categories"));
            serializer.Serialize(new StringWriter(sb), categories);

            return sb.ToString().TrimEnd();
        }

        //Exercise 8 Users and Products
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context
                .Users
                .Where(u => u.ProductsSold.Any())              
                .Select(u => new ExportUserDTO
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age,
                    SoldProducts = new ExportProductCountDTO
                    {
                        Count = u.ProductsSold.Count,
                        Products = u.ProductsSold.Select(ps => new ExportProductDTO
                        {
                            Name = ps.Name,
                            Price = ps.Price
                        })
                        .OrderByDescending(ps => ps.Price)
                        .ToArray()
                    }
                })
                .OrderByDescending(u => u.SoldProducts.Count)
                .Take(10)
                .ToArray();

            var result = new ExportUserCountDTO
            {
                Count = context.Users.Count(u => u.ProductsSold.Any()),
                Users = users
            };

            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(ExportUserCountDTO), new XmlRootAttribute("Users"));
            serializer.Serialize(new StringWriter(sb), result);

            return sb.ToString().TrimEnd();
        }
    }
}