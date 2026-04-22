using FoodProject.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FoodProject.ViewComponents
{
	public class FoodListByCategory : ViewComponent
	{
		public IViewComponentResult Invoke(int id)
		{
			
			FoodRepository foodRepository = new FoodRepository();
			var foodList = foodRepository.List(x => x.CategoryID == id); 
			return View(foodList);
		}
	}
}
