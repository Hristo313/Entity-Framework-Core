using System;
using System.Collections.Generic;
using System.Text;

namespace _2.FootballBetting.Data.Models
{
    public class Country
    {
        public Country()
        {
            this.Towns = new HashSet<Town>();
        }

        public int CountryId { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Town> Towns { get; set; }
    }
}
