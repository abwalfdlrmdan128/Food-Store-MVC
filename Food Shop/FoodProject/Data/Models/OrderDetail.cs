using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodProject.Data.Models
{
    public class OrderDetail
    {
        [Key]
        public int OrderDetailID { get; set; }
        public string FoodName { get; set; }
        public double FoodPrice { get; set; }
        public string FoodImage { get; set; }
        public int FoodQuantity { get; set; }
        public DateTime FoodOrderDate { get; set; }
        public int FoodStock { get; set; }
        public int OrderID { get; set; }
        public Order Order { get; set; }

        public int FoodID { get; set; }
        public Food Food { get; set; }
    }
}
