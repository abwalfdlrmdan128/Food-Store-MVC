using FoodProject.Data;
using FoodProject.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FoodProject.Controllers
{
    public class OrderController : Controller
    {
        Context context = new Context();

        //  Add to Cart
        public IActionResult Index(int id)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Food");

            var userName = User.Identity.Name;
            var userId = context.Users
                .Where(x => x.UserName == userName)
                .Select(x => x.Id)
                .FirstOrDefault();

            var food = context.Foods.Find(id);
            if (food == null) return NotFound();

            var existing = context.Shoppings
                .FirstOrDefault(x => x.FoodID == id && x.AppUserID == userId);

            if (existing != null)
            {
                existing.ShoppingQuantity += 1;
            }
            else
            {
                context.Shoppings.Add(new Shopping
                {
                    FoodID = id,
                    AppUserID = userId,
                    ShoppingQuantity = 1,
                    ShoppingPrice = food.Price
                });
            }

            context.SaveChanges();
            return RedirectToAction("Index", "Default");
        }

        //  Basket
        public IActionResult BasketDetails()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");

            var userName = User.Identity.Name;
            var userId = context.Users
                .Where(x => x.UserName == userName)
                .Select(x => x.Id)
                .FirstOrDefault();

            var basket = context.Shoppings
                .Where(x => x.AppUserID == userId)
                .Include(x => x.Food)
                .ToList();

            ViewBag.TotalPrice = basket.Sum(x => x.ShoppingPrice * x.ShoppingQuantity);
            ViewBag.BasketCount = basket.Count;

            return View(basket);
        }

        //  Delete
        public IActionResult DeleteProduct(int id)
        {
            var item = context.Shoppings.Find(id);
            if (item != null)
            {
                context.Shoppings.Remove(item);
                context.SaveChanges();
            }
            return RedirectToAction("BasketDetails");
        }

        //  Plus
        public IActionResult PlusProduct(int id)
        {
            var item = context.Shoppings.Find(id);
            if (item != null)
            {
                item.ShoppingQuantity++;
                context.SaveChanges();
            }
            return RedirectToAction("BasketDetails");
        }

        //  Minus (fixed bug)
        public IActionResult MinusProduct(int id)
        {
            var item = context.Shoppings.Find(id);
            if (item != null && item.ShoppingQuantity > 1)
            {
                item.ShoppingQuantity--;
                context.SaveChanges();
            }
            return RedirectToAction("BasketDetails");
        }

        //  Payment Page
        [HttpGet]
        public IActionResult PaymentAdd()
        {
            return View();
        }

        //  Checkout ()
        [HttpPost]
        public IActionResult PaymentAdd(Payment payment)
        {
            if (!ModelState.IsValid)
                return View(payment);

            var userName = User.Identity.Name;
            var userId = context.Users
                .Where(x => x.UserName == userName)
                .Select(x => x.Id)
                .FirstOrDefault();

            var cart = context.Shoppings
                .Where(x => x.AppUserID == userId)
                .Include(x => x.Food)
                .ToList();

            if (!cart.Any())
                return BadRequest("Cart is empty");

            //  Calculate total
            double total = cart.Sum(x => x.ShoppingQuantity * x.Food.Price);

            //  Save Payment
            payment.AppUserID = userId;
            payment.ShoppingTotal = total;

            context.Payments.Add(payment);
            context.SaveChanges();

            //  Create Order
            var order = new Order
            {
                AppUserID = userId,
                PaymentId = payment.PaymentId,
                TotalPrice = total,
                Status = "Completed"
            };

            context.Orders.Add(order);
            context.SaveChanges();

            //  Create OrderDetails
            var orderDetails = cart.Select(item => new OrderDetail
            {
                OrderID = order.OrderID,
                FoodID = item.FoodID,
                FoodName = item.Food.Name,
                FoodPrice = item.Food.Price,
                FoodImage = item.Food.ImageURL,
                FoodQuantity = item.ShoppingQuantity
            }).ToList();

            context.OrderDetails.AddRange(orderDetails);

            //  Update Stock
            foreach (var item in cart)
            {
                item.Food.Stock -= item.ShoppingQuantity;
            }

            //  Clear cart
            context.Shoppings.RemoveRange(cart);

            context.SaveChanges();

            return RedirectToAction("UserOrders");
        }

        // User Orders
        public IActionResult UserOrders()
        {
            var userName = User.Identity.Name;
            var userId = context.Users
                .Where(x => x.UserName == userName)
                .Select(x => x.Id)
                .FirstOrDefault();

            var orders = context.Orders
                .Where(x => x.AppUserID == userId)
                .Include(x => x.OrderDetails)
                .ToList();

            return View(orders);
        }
    }
}