using System;
using System.Collections.Generic;

namespace FoodProject.Data.Models
{
    public class Order
    {
        public int OrderID { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public double TotalPrice { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Completed, Cancelled

        // Relations
        public int AppUserID { get; set; }
        public AppUser AppUser { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }

        public int PaymentId { get; set; }
        public Payment Payment { get; set; }
    }
}
