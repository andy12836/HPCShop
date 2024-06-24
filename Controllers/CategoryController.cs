using Bulky.DataAccess;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;

namespace BulkyWeb.Controllers
{
    public class CategoryController : Controller
    {
        private ICategoryRepository _categoryRepo;
        public CategoryController(ICategoryRepository db)
        {
            _categoryRepo = db;
        }
        public IActionResult Index()
        {
            List<Category> objCategoryList = _categoryRepo.GetAll().ToList();
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
                _categoryRepo.Add(category);
                _categoryRepo.Save();
                TempData["create"] = "Data created successfully!";
                return RedirectToAction("index");
            }
            return View();
        }


        // 參數categoryId會由 Index.cshtml中傳遞 使用 asp-route-categoryId="@obj.Id"
        public IActionResult Edit(int? categoryId)
        {
            if(categoryId==null || categoryId == 0) { return NotFound(); }
            
            // there are three ways to search in datatbase
            // the "find()" can be only used to find by its key
            Category? category = _categoryRepo.Get(u=>u.Id==categoryId);
            // Category? category2 = _db.Categories.FirstOrDefault(u=>u.Id==categoryId);
            // Category? category3 = _db.Categories.Where(u => u.Id == categoryId).FirstOrDefault();
            
            if(category == null) { return NotFound(); }

            return View(category);
        }

        // 參數category物件的屬性會由 Edit.cshtml 傳遞，在 <form> 中使用標籤 <input asp-for="Id"/> 等建立屬性

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _categoryRepo.Update(category);
                _categoryRepo.Save();
                TempData["edit"] = "Data edited successfully!";
                return RedirectToAction("index");
            }
            return View();
        }

        // 參數categoryId2會由 Index.cshtml中傳遞 使用 asp-route-categoryId2="@obj.Id"

        public IActionResult Delete(int? categoryId2)
        {
            if (categoryId2 == null || categoryId2 == 0) { return NotFound(); }


            Category? category = _categoryRepo.Get(u=>u.Id==categoryId2);


            if (category == null) { return NotFound(); }

            return View(category);
        }

        // 參數category物件的屬性會由 Delete.cshtml 傳遞，在 <form> 中使用標籤 <input asp-for="Id"/> 等建立屬性
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? categoryId2)
        {
            if (categoryId2 == null) return NotFound();
            Category obj = _categoryRepo.Get(u => u.Id == categoryId2);
            if (obj == null) return NotFound();

            _categoryRepo.Remove(obj);
            _categoryRepo.Save();
            TempData["delete"] = "Data deleted successfully!";
            return RedirectToAction("index");
        }



    }
}
