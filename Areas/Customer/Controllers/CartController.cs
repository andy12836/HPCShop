using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private ShoppingCartVM ShoppingCartVM;

        public CartController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;

        }

        [Authorize]
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new ShoppingCartVM()
            {
                CartList = _unitOfWork.ShoppingCart.GetAll(filter: u => u.AppUserId == userId,
                includeProperties: "Product")
            };

            // count the sum of price in shopping cart
            foreach(var shoppingCart in ShoppingCartVM.CartList)
            {
                shoppingCart.PriceSum = GetPriceBasedOnQuantity(shoppingCart) * shoppingCart.Count;
                ShoppingCartVM.OrderTotal += shoppingCart.PriceSum;
            }

            return View(ShoppingCartVM);
        }


        public IActionResult Plus(int cartId) {
            ShoppingCart cart = _unitOfWork.ShoppingCart.Get(u=>u.Id == cartId);
            if (cart != null)
            {
                cart.Count++;
                _unitOfWork.ShoppingCart.Update(cart);
                _unitOfWork.Save();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Minus(int cartId)
        {
            ShoppingCart cart = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            if (cart.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cart);
                _unitOfWork.Save();
            }
            else
            {
                cart.Count--;
                _unitOfWork.ShoppingCart.Update(cart);
                _unitOfWork.Save();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int cartId)
        {
            ShoppingCart cart = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            if (cart != null)
            {
                _unitOfWork.ShoppingCart.Remove(cart);
                _unitOfWork.Save();
            }
            return RedirectToAction("Index");
        }


        public IActionResult Summary()
        {
            return View();
        }

        private double GetPriceBasedOnQuantity (ShoppingCart shoppingCart)
        {
            if(shoppingCart.Count < 50) return shoppingCart.Product.Price;
            else if(shoppingCart.Count < 100) return shoppingCart.Product.Price50;
            else return shoppingCart.Product.Price100;
        }
    }
}
