using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Newtonsoft.Json;
using ProductShop.Data;
using ProductShop.DTO.Category;
using ProductShop.DTO.Product;
using ProductShop.DTO.Users;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        private const string ResultDirectoryPath = "../../../Datasets/Results";
        private static MapperConfiguration config;

        public static void Main(string[] args)
        {
            ProductShopContext db = new ProductShopContext();

            //ResetDatabase(db)

            config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductShopProfile>();
            });

            // Exercise 1-4 Import
            // string inputJson = File.ReadAllText("../../../Datasets/categories-products.json"); 

            string json = GetUsersWithProducts(db);

            EnsureDirectoryExists(ResultDirectoryPath);

            File.WriteAllText(ResultDirectoryPath + "/users-and-products.json", json);
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static void ResetDatabase(ProductShopContext db)
        {
            db.Database.EnsureDeleted();
            Console.WriteLine("Database was successfully deleted!");
            db.Database.EnsureCreated();
            Console.WriteLine("Database was successfully created!");
        }

        //Exercise 1.Import Users
        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            User[] users = JsonConvert.DeserializeObject<User[]>(inputJson);

            //! With DTO !
            //UsersImportDTO[] usersDTO = JsonConvert.DeserializeObject<UsersImportDTO[]>(inputJson);
            //User[] users = usersDTO.Select(u => Mapper.Map<User>(u)).ToArray();

            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Length}";
        }

        //Exercise 2.Import Products
        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            Product[] products = JsonConvert.DeserializeObject<Product[]>(inputJson);

            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Length}";
        }

        //Exercise 3.Import Categories
        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            //JsonSerializerSettings settings = new JsonSerializerSettings()
            //{
            //    NullValueHandling = NullValueHandling.Ignore
            //};

            Category[] categories = JsonConvert.DeserializeObject<Category[]>(inputJson)
                .Where(c => c.Name != null)
                .ToArray();

            context.Categories.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Length}";
        }

        //Exercise 4.Import Categories and Products
        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            CategoryProduct[] categoryProducts = JsonConvert.DeserializeObject<CategoryProduct[]>(inputJson);

            context.CategoryProducts.AddRange(categoryProducts);
            context.SaveChanges();

            return $"Successfully imported {categoryProducts.Length}";
        }

        //Exercise 5.Export Products in Range
        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context
                .Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .OrderBy(p => p.Price)
                //.Select(p => new ListProductInRangeDTO
                //{
                //    Name = p.Name,
                //    Price = p.Price,     
                //    Seller = p.Seller.FirstName + " " + p.Seller.LastName
                //})
                .ProjectTo<ListProductInRangeDTO>(config)
                .ToArray();

            string json = JsonConvert.SerializeObject(products, Formatting.Indented);

            return json;
        }

        //Exercise 6.Export Successfully Sold Products
        public static string GetSoldProducts(ProductShopContext context)
        {
            var users = context
                .Users
                .Where(u => u.ProductsSold.Any(p => p.Buyer != null))
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                //.Select(u => new
                //{
                //    firstName = u.FirstName,
                //    lastName = u.LastName,
                //    soldProducts = u.ProductsSold
                //                                  .Where(p => p.Buyer != null)
                //                                  .Select(p => new
                //                                            { 
                //                                                 name = p.Name,
                //                                                 price = p.Price,
                //                                                 buyerFirstName = p.Buyer.FirstName,
                //                                                 buyerLastName = p.Buyer.LastName
                //                                             })
                //                                  .ToArray()
                //})
                .ProjectTo<UserWithSoldProductsDTO>(config)
                .ToArray();

            string json = JsonConvert.SerializeObject(users, Formatting.Indented);

            return json;
        }

        //Exercise 7.Export Categories by Products Count
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context
                .Categories
                .OrderByDescending(c => c.CategoryProducts.Count)
                /*.Select(c => new
                { 
                    category = c.Name,
                    productsCount = c.CategoryProducts.Count,
                    averagePrice = c.CategoryProducts.Average(cp => cp.Product.Price).ToString("F2"),
                    totalPriceSum = c.CategoryProducts.Sum(cp => cp.Product.Price).ToString("F2")
                })*/
                .ProjectTo<CategoriesByProductsCountDTO>(config)
                .ToArray();

            string json = JsonConvert.SerializeObject(categories, Formatting.Indented);

            return json;
        }

        //Exercise 8.Export Users and Products
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context
                .Users
                .Where(u => u.ProductsSold.Any(p => p.Buyer != null))
                .OrderByDescending(u => u.ProductsSold.Count(p => p.Buyer != null))
                .Select(u => new
                {
                    lastName = u.LastName,
                    age = u.Age,
                    soldProducts = new
                    {
                        count = u.ProductsSold.Count(p => p.Buyer != null),
                        products = u.ProductsSold.Where(p => p.Buyer != null).Select(p => new
                        {
                            name = p.Name,
                            price = p.Price
                        })
                       .ToArray()
                    }
                })
                .ToArray();

            var resultObj = new
            {
                usersCount = users.Length,
                users = users
            };

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(resultObj, settings);

            return json;
        }
    }
}