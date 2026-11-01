using FoodProject.Data;
using FoodProject.Data.Models;
using FoodProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FoodProject.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        Context context = new Context();
        private readonly IDatabase _database;
        private readonly PayPalClient _payPalClient;
        public OrderController(PayPalClient payPalClient, IConnectionMultiplexer redis)
        {
            _payPalClient = payPalClient;
            _database = redis.GetDatabase();
        }
        //  Add to Cart
       
        public IActionResult Index(int id)
        {
            if (!User.Identity.IsAuthenticated)return RedirectToAction("Index", "Login");

            var userName = User.Identity.Name;

            var userId = context.Users
                .Where(x => x.UserName == userName)
                .Select(x => x.Id)
                .FirstOrDefault();

            /*
             List<Shopping> basket;

            if (basketJson.IsNullOrEmpty)
          {
                basket = new List<Shopping>();
            }
            else
          {
    basket = JsonSerializer.Deserialize<List<Shopping>>(
        basketJson.ToString()
    );

    if (basket == null)
    {
        basket = new List<Shopping>();
    }
}
             */
            var food = context.Foods.Find(id);
            if (food == null) return NotFound();

            // Get user's basket from Redis
            var key = $"basket:{userId}";

            var basketJson = _database.StringGet(key);

            var basket = basketJson.IsNullOrEmpty ? new List<Shopping>() : JsonSerializer.Deserialize<List<Shopping>>(basketJson.ToString()) ?? new List<Shopping>();

            // Check if food already exists
            var existing = basket.FirstOrDefault(x => x.FoodID == id);

            if (existing != null)
            {
                existing.ShoppingQuantity += 1;
            }
            else
            {
                basket.Add(new Shopping
                {
                    FoodID = id,
                    AppUserID = userId,
                    ShoppingQuantity = 1,
                    ShoppingPrice = food.Price
                });
            }

            // Save basket in Redis
            _database.StringSet(key, JsonSerializer.Serialize(basket));

            return RedirectToAction("Index", "Default");
        }

        // Basket
        public IActionResult BasketDetails()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Index", "Login");

            var userName = User.Identity.Name;

            var userId = context.Users
                .Where(x => x.UserName == userName)
                .Select(x => x.Id)
                .FirstOrDefault();

            // Get basket from Redis
            var key = $"basket:{userId}";
            var basketJson = _database.StringGet(key);

            var basket = basketJson.IsNullOrEmpty ? new List<Shopping>(): JsonSerializer.Deserialize<List<Shopping>>(basketJson.ToString()) ?? new List<Shopping>();

            // Load Food data from SQL
            foreach (var item in basket)
            {
                item.Food = context.Foods.Find(item.FoodID);
            }

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


        [HttpGet]
        public IActionResult PaymentAdd()
        {
            ViewBag.ClientId = _payPalClient.ClientId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaypalOrder([FromBody] ShippingDto dto)
        {
            var userId = context.Users
                .Where(x => x.UserName == User.Identity.Name)
                .Select(x => x.Id)
                .FirstOrDefault();

            var cart = context.Shoppings
                .Where(x => x.AppUserID == userId)
                .Include(x => x.Food)
                .ToList();

            if (!cart.Any())
                return BadRequest("Cart empty");

            double total = cart.Sum(x => x.ShoppingQuantity * x.Food.Price);

            TempData["Mobile"] = dto.Mobile;
            TempData["City"] = dto.City;
            TempData["Address"] = dto.Address;

            var order = await _payPalClient.CreateOrder(
                total.ToString("F2"),
                "USD",
                Guid.NewGuid().ToString()
            );

            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> CapturePaypalOrder(string orderId)
        {
            var result = await _payPalClient.CaptureOrder(orderId);

            var userId = context.Users
                .Where(x => x.UserName == User.Identity.Name)
                .Select(x => x.Id)
                .FirstOrDefault();

            var cart = context.Shoppings
                .Where(x => x.AppUserID == userId)
                .Include(x => x.Food)
                .ToList();

            double total = cart.Sum(x => x.ShoppingQuantity * x.Food.Price);

            // Payment
            var payment = new Payment
            {
                AppUserID = userId,
                ShoppingTotal = total,
                MobileNumber = TempData["Mobile"]?.ToString(),
                City = TempData["City"]?.ToString(),
                Address = TempData["Address"]?.ToString(),
                Email = User.Identity.Name
            };

            context.Payments.Add(payment);
            context.SaveChanges();

            // Order
            var order = new Order
            {
                AppUserID = userId,
                PaymentId = payment.PaymentId,
                TotalPrice = total,
                Status = "Pending"
            };

            context.Orders.Add(order);
            context.SaveChanges();

            // Details
            context.OrderDetails.AddRange(
                cart.Select(x => new OrderDetail
                {
                    OrderID = order.OrderID,
                    FoodID = x.FoodID,
                    FoodName = x.Food.Name,
                    FoodPrice = x.Food.Price,
                    FoodQuantity = x.ShoppingQuantity
                })
            );

            // stock
            foreach (var item in cart)
                item.Food.Stock -= item.ShoppingQuantity;

            context.Shoppings.RemoveRange(cart);
            context.SaveChanges();

            return Ok(result);
        }
    }
}

