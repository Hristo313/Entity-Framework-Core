namespace BookShop.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text;

    public class Category
    {
        public Category()
        {
            this.BookCategories = new HashSet<BookCategory>();
        }

        [Key]
        public int CategoryId { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }

        public virtual ICollection<BookCategory> BookCategories { get; set; }
    }
}
