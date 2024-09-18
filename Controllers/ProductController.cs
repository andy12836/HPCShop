using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BulkyWeb.Controllers
{
    public class ProductController : Controller
    {

        private IUnitOfWork _unitOfWork;

        // constructor
        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll().ToList();
            return View(objProductList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product product)
        {

            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Add(product);
                _unitOfWork.Save();
                TempData["create"] = "Data created successfully!";
                return RedirectToAction("index");
            }
            return View();
        }

        public IActionResult Edit(int? productId)
        {
            if (productId == null || productId == 0) { return NotFound(); }

            // there are three ways to search in datatbase
            // the "find()" can be only used to find by its key
            Product? product = _unitOfWork.Product.Get(u => u.Id == productId);
            // Category? category2 = _db.Categories.FirstOrDefault(u=>u.Id==categoryId);
            // Category? category3 = _db.Categories.Where(u => u.Id == categoryId).FirstOrDefault();
            
            if (product == null) { return NotFound(); }

            return View(product);
        }

        // 參數category物件的屬性會由 Edit.cshtml 傳遞，在 <form> 中使用標籤 <input asp-for="Id"/> 等建立屬性

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Update(product);
                _unitOfWork.Save();
                TempData["edit"] = "Data edited successfully!";
                return RedirectToAction("index");
            }
            return View();
        }

        // 參數categoryId2會由 Index.cshtml中傳遞 使用 asp-route-categoryId2="@obj.Id"

        public IActionResult Delete(int? productId2)
        {
            if (productId2 == null || productId2 == 0) { return NotFound(); }


            Product? product= _unitOfWork.Product.Get(u => u.Id == productId2);


            if (product == null) { return NotFound(); }

            return View(product);
        }

        // 參數category物件的屬性會由 Delete.cshtml 傳遞，在 <form> 中使用標籤 <input asp-for="Id"/> 等建立屬性
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? productId2)
        {
            if (productId2 == null) return NotFound();
            Product obj = _unitOfWork.Product.Get(u => u.Id == productId2);
            if (obj == null) return NotFound();

            _unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();
            TempData["delete"] = "Data deleted successfully!";
            return RedirectToAction("index");
        }
    }
}
