using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BulkyWeb.Controllers
{
    public class ProductController : Controller
    {

        private IUnitOfWork _unitOfWork;
        private IWebHostEnvironment _webHostEnvironment;

        // constructor
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHost)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHost;
        }

        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
   


            return View(objProductList);
        }


        // combine create and update into upsert
        public IActionResult Upsert(int? productId)
        {

            // convert Category data to a drop down list (by EF projection)
            // IEnumerable<SelectListItem> takes in Text and Value
            // press F12 on "SelectListItem to check the property"
            //IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(
                //u => new SelectListItem
                //{
                //    Text = u.Name,
                //    Value = u.Id.ToString(),
                //});

            // ViewBag transfers data from controller to view.
            // ViewBag's life only exists during the current http request
            // when redirect happens, viewbag losts data
            // use <asp-items="ViewBag.[Property]"> to fetch data in view
            //ViewBag.CategoryList = CategoryList;

            //use ViewModel to transfer data
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategoryList = _unitOfWork.Category.GetAll().Select(
                u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                })
            };

            if(productId == null || productId == 0)
            {
                //create
                return View(productVM);
            }
            else
            {
                //update
                productVM.Product = _unitOfWork.Product.Get(u=>u.Id == productId);
                return View(productVM);
            }
            
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {

            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    // generate a random file name
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(obj.Product.ImageUrl))
                    {
                        // delete old image
                        var oldImgPath = 
                            Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImgPath))
                        {
                            System.IO.File.Delete(oldImgPath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    obj.Product.ImageUrl = @"\images\product\" + fileName;
                }

                if (obj.Product.Id == 0)
                {
                    // create
                    _unitOfWork.Product.Add(obj.Product);
                }
                else
                {
                    // update
                    _unitOfWork.Product.Update(obj.Product);
                }

                
                _unitOfWork.Save();
                TempData["create"] = "Data created successfully!";
                return RedirectToAction("index");
            }
            else
            {
                obj.CategoryList = _unitOfWork.Category.GetAll().Select(
                u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                });
                return View(obj);
            };
            
        }
        

        // method "Edit" is combined into "Upsert"
        //public IActionResult Edit(int? productId)
        //{
        //    if (productId == null || productId == 0) { return NotFound(); }

        //    // there are three ways to search in datatbase
        //    // the "find()" can be only used to find by its key
        //    Product? product = _unitOfWork.Product.Get(u => u.Id == productId);
        //    // Category? category2 = _db.Categories.FirstOrDefault(u=>u.Id==categoryId);
        //    // Category? category3 = _db.Categories.Where(u => u.Id == categoryId).FirstOrDefault();
            
        //    if (product == null) { return NotFound(); }

        //    return View(product);
        //}

        //// 參數category物件的屬性會由 Edit.cshtml 傳遞，在 <form> 中使用標籤 <input asp-for="Id"/> 等建立屬性

        //[HttpPost]
        //public IActionResult Edit(Product product)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _unitOfWork.Product.Update(product);
        //        _unitOfWork.Save();
        //        TempData["edit"] = "Data edited successfully!";
        //        return RedirectToAction("index");
        //    }
        //    return View();
        //}

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
