﻿using RealEstates.Data;
using RealEstates.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RealEstates.Services
{
    public class DistrictsService : IDistrictService
    {
        private RealEstateDbContext db;

        public DistrictsService(RealEstateDbContext db)
        {
            this.db = db;
        }

        public IEnumerable<DistrictViewModel> GetTopDistrictsByAveragePrice(int count = 10)
        {
            return this.db.Districts
                .OrderByDescending(x => x.Properties.Average(x => (double)x.Price / x.Size))
                .Select(MapToDistrictViewModel())
                .Take(count)
                .ToList();
        }

        public IEnumerable<DistrictViewModel> GetTopDistrictsByNumberOfProperties(int count = 10)
        {
            return this.db.Districts
                .OrderByDescending(x => x.Properties.Count())
                .Select(MapToDistrictViewModel())
                .Take(count)
                .ToList();
        }

        private static Expression<Func<District, DistrictViewModel>> MapToDistrictViewModel()
        {
            return x => new DistrictViewModel
            {
                Name = x.Name,
                AveragePrice = x.Properties.Average(x => (double)x.Price / x.Size),
                MinPrice = x.Properties.Min(x => x.Price),
                MaxPrice = x.Properties.Max(x => x.Price),
                PropertiesCount = x.Properties.Count()
            };
        }
    }
}
