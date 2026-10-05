using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTO
{
    public class CategoryVM
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
    public class UpdateCategoryVM
    {
        public string CategoryName { get; set; }
    }
}
