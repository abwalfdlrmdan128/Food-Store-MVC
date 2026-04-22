using FoodProject.Data;
using FoodProject.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;

namespace FoodProject.Controllers
{

    public class AboutController : Controller
    {
        Context context = new Context();

        // GET: AboutController all abouts
        [HttpGet]
        public IActionResult Index()
        {
            var about = context.Abouts.ToList();
            //TempData["Success"]=null;
            return View(about);
        }
        // GET: AboutController/Details/id
        [HttpGet]
        public IActionResult AboutUpdate(int id)
        {
            var aboutID = context.Abouts.Find(id);
            return View(aboutID);
        }
        // POST: AboutController/Details/id
        [HttpPost]
        public IActionResult AboutUpdate(AboutImage p)
        {
            About about = new About();
            var value = context.Abouts.Find(p.AboutID);
            if (value != null)
            {
                context.Entry(value).State = EntityState.Detached;
                if (p.AboutImageURL != null)
                {
                    var extension = Path.GetExtension(p.AboutImageURL.FileName);
                    var newImageName = Guid.NewGuid() + extension;
                    var location = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Resimler/", newImageName);
                    var stream = new FileStream(location, FileMode.Create);
                    p.AboutImageURL.CopyTo(stream);
                    about.AboutImageURL = newImageName;
                }
                else
                {
                   //if no new image add the old
                   about.AboutImageURL = value.AboutImageURL;                  
                }
                 about.AboutID = p.AboutID;
                about.AboutTitle = p.AboutTitle;
                about.AboutText = p.AboutText;

                context.Abouts.Update(about);
                context.SaveChanges();

                TempData["Success"] = "Updated successfully!";
            }
            return RedirectToAction("Index", "About");
        }
        // GET: AboutController/Delete/id
        [HttpDelete]
        public IActionResult AboutDelete(int id)
        {
            var value = context.Abouts.Find(id);
            if (value != null)
            {
                context.Abouts.Remove(value);
                context.SaveChanges();
                TempData["Success"] = "Deleted successfully!!";
            }
            return RedirectToAction("Index", "About");
        }
        // GET: AboutController/Create
        [HttpGet]
        public IActionResult AboutAdd()
        {
            return View();
        }
        // POST: AboutController/Create
        [HttpPost]
        public IActionResult AboutAdd(AboutImage p)
        {
            About about = new About();
            if (p.AboutImageURL != null)
            {
                var extension = Path.GetExtension(p.AboutImageURL.FileName);
                var newImageName = Guid.NewGuid() + extension;
                var location = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Resimler/", newImageName);
                var stream = new FileStream(location, FileMode.Create);
                p.AboutImageURL.CopyTo(stream);
                about.AboutImageURL = newImageName;
            }
            about.AboutTitle = p.AboutTitle;
            about.AboutText = p.AboutText;
            context.Abouts.Add(about);
            context.SaveChanges();
            TempData["Success"] = "Added successfully!";
            return RedirectToAction("Index", "About");
        }

        [HttpGet]
        public IActionResult AboutDetails(int id)
        {
            var AboutId = context.Abouts.Find(id);
            return View(AboutId);
        }
    }
}
