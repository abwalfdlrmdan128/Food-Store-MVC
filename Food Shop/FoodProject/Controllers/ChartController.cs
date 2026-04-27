using FoodProject.Data;
using FoodProject.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.Linq;

namespace FoodProject.Controllers
{
	 
    [Authorize(Roles = "Admin")]
    
    public class ChartController : Controller
    {
        // Static Google Chart
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }



        [HttpGet]
        public IActionResult Index2()
        {
            return View();
        }
        public IActionResult VisualizeProductResult()
        {
            return Json(CategoryLis());
        }
        // Dynamic Google Chart (categories)
        public List<Class1> CategoryLis()
        {
            List<Class1> cs = new List<Class1>();
            using (var context = new Context())
            {
                cs = context.Categories.Select(x => new Class1
                {
                    CategoryName =x.CategoryName,
                    stock =x.Foods.Count()
                }).ToList();
            }
            return cs;
        }




        // Dynamic Google Chart (Foods)
        public IActionResult Index3()
        {
            return View();
        }
        public IActionResult VisualizeFoodResult()
        {
            return Json(FoodList());
        }
        public List<Class2> FoodList()
        {
            List<Class2> cs2=new List<Class2>();
            using(var context=new Context())
            {
                cs2 = context.Foods.Select(x => new Class2
                {
                    foodName = x.Name,
                    stock = x.Stock
                }).ToList();
            }
            return cs2;
        }



        // Statistics
        public IActionResult Statistics()
        {
            Context context = new Context();

            var foodCount = context.Foods.Count();
            ViewBag.foodCount = foodCount;

            var categoryCount = context.Categories.Count();
            ViewBag.categoryCount = categoryCount;

            var vegetableCategoryId = context.Categories
                .Where(x => x.CategoryName.ToLower() == "vegetables")
                .Select(y => y.CategoryID)
                .FirstOrDefault();

            var vegetableCount = context.Foods
                .Where(x => x.CategoryID == vegetableCategoryId)
                .Count();
            ViewBag.vegetableCount = vegetableCount;

            var orderCount = context.Payments.Count();
            ViewBag.orderCount = orderCount;

            var totalStock = context.Foods.Sum(x => x.Stock);
            ViewBag.totalStock = totalStock;

            var userCount = context.Users.Count();
            ViewBag.userCount = userCount;

            var maxStockFood = context.Foods
                .OrderByDescending(x => x.Stock)
                .Select(y => y.Name)
                .FirstOrDefault();
            ViewBag.maxStockFood = maxStockFood;

            var minStockFood = context.Foods
                .OrderBy(x => x.Stock)
                .Select(y => y.Name)
                .FirstOrDefault();
            ViewBag.minStockFood = minStockFood;

            var averageFoodPrice = context.Foods
                .Average(x => x.Price)
                .ToString("0.00");
            ViewBag.averageFoodPrice = averageFoodPrice;

            var fruitCategoryId = context.Categories
                .Where(x => x.CategoryName.ToLower() == "fruits")
                .Select(y => y.CategoryID)
                .FirstOrDefault();

            var totalFruitStock = context.Foods
                .Where(y => y.CategoryID == fruitCategoryId)
                .Sum(x => x.Stock);
            ViewBag.totalFruitStock = totalFruitStock;

            var totalVegetableStock = context.Foods
                .Where(y => y.CategoryID == vegetableCategoryId)
                .Sum(x => x.Stock);
            ViewBag.totalVegetableStock = totalVegetableStock;

            var mostExpensiveFood = context.Foods
                .OrderByDescending(x => x.Price)
                .Select(y => y.Name)
                .FirstOrDefault();
            ViewBag.mostExpensiveFood = mostExpensiveFood;

            return View();
        }
    }
}
