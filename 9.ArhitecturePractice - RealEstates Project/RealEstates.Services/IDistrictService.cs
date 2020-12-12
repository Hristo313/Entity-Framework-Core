using System;
using System.Collections.Generic;
using System.Text;

namespace RealEstates.Services
{
    public interface IDistrictService
    {
        IEnumerable<DistrictViewModel> GetTopDistrictsByAveragePrice(int count = 10);

        IEnumerable<DistrictViewModel> GetTopDistrictsByNumberOfProperties(int count = 10);
    }
}
