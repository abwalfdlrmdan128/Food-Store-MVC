using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodProject.Data.Models
{
	public class AppUser:IdentityUser<int>
	{
		public string NameSurname { get; set; }
    
        public List<Shopping> Shoppings { get; set; }
      
        public List<Payment> Payments { get; set; }
        
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
