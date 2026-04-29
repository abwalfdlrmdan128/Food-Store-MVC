using FoodProject.Data;
using FoodProject.Data.Models;
using FoodProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FoodProject.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        Context context = new Context();
        private readonly PayPalClient _payPalClient;
        public OrderController(PayPalClient payPalClient)
        {
            _payPalClient = payPalClient;
        }
        //  Add to Cart
        public IActionResult Index(int id)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login");

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