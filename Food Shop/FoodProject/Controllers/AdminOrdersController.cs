using FoodProject.Data;
using FoodProject.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using X.PagedList;

namespace FoodProject.Controllers
{
    // [Authorize(Roles = "Admin")]
    public class AdminOrdersController : Controller
    {
        Context context = new Context();

        // ✅ Get All Orders 
        [HttpGet]
        public IActionResult Index(int page = 1)
        {
            var orders = context.Orders
                .Include(x => x.AppUser)
                .Include(x => x.Payment)
                .OrderByDescending(x => x.OrderDate)
                .ToList();

            return View(orders.ToPagedList(page, 8));
        }

        // ✅ Order Details
        [HttpGet]
        public IActionResult OrderDetails(int id)
        {
            var order = context.Orders
                .Include(x => x.AppUser)
                .Include(x => x.Payment)
                .Include(x => x.OrderDetails)
                .ThenInclude(od => od.Food)
                .FirstOrDefault(x => x.OrderID == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // ✅ Complete Order 
        [HttpPost]
        public IActionResult OrderCompleted(int id)
        {
            var order = context.Orders.Find(id);

            if (order == null)
                return NotFound();

            order.Status = "Completed";

            context.SaveChanges();

            return RedirectToAction("Index");
        }

        // Cancel Order
        [HttpPost]
        public IActionResult CancelOrder(int id)
        {
            var order = context.Orders
                .Include(x => x.OrderDetails)
                .ThenInclude(od => od.Food)
                .FirstOrDefault(x => x.OrderID == id);

            if (order == null)
                return NotFound();

            
            foreach (var item in order.OrderDetails)
            {
                item.Food.Stock += item.FoodQuantity;
            }

            order.Status = "Cancelled";

            context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}