using AutoMapper;
using ProductShop.DTO.Category;
using ProductShop.DTO.Product;
using ProductShop.DTO.Users;
using ProductShop.Models;
using System.Linq;

namespace ProductShop
{
    public class ProductShopProfile : Profile
    {
        public ProductShopProfile()
        {
            this.CreateMap<Product, ListProductInRangeDTO>()
                .ForMember(x => x.Name, y => y.MapFrom(x => x.Seller.FirstName + " " + x.Seller.LastName));

            this.CreateMap<User, UserSoldProductDTO>();
            this.CreateMap<User, UserWithSoldProductsDTO>()
                .ForMember(x => x.SoldProducts, y => y
                .MapFrom(x => x.ProductsSold.Where(p => p.Buyer != null)));

            this.CreateMap<Category, CategoriesByProductsCountDTO>()
                .ForMember(x => x.Category, y => y.MapFrom(x => x.Name))
                .ForMember(x => x.ProductsCount, y => y.MapFrom(x => x.CategoryProducts.Count))
                .ForMember(x => x.AveragePrice, y => y.MapFrom(x => x.CategoryProducts.Average(cp => cp.Product.Price).ToString("F2")))
                .ForMember(x => x.TotalRevenue, y => y.MapFrom(x => x.CategoryProducts.Sum(cp => cp.Product.Price).ToString("F2")));

            // Import Users with DTO !
            //this.CreateMap<UsersImportDTO, User>();
        }
    }
}
