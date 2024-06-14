using BulkyWeb.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;

namespace BulkyWeb.Controllers
{
    public class CategoryController : Controller
    {
        private ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            List<Category> objCategoryList = _db.Categories.ToList();
            return View(objCategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            // validation
            if(!category.Name.IsNullOrEmpty() && category.Name.ToLower() == category.DisplayOrder.ToString())
            {
                // "name" should match "asp-for" in the View
                ModelState.AddModelError("name", "Category Name should not match Display Order");
            }

            // validation
            if (!category.Name.IsNullOrEmpty() && category.Name.ToLower() == "test")
            {
                // The error message will be displayed only when "asp-validation-summary = all"
                ModelState.AddModelError("", "\"Test\" is not a valid name");
            }

            if (ModelState.IsValid) { 
                _db.Categories.Add(category);
                _db.SaveChanges();
                return RedirectToAction("index");
            }
            return View();
        }


    }
}
