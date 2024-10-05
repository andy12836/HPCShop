using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

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
                includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };

            // count the sum of price in shopping cart
            foreach(var shoppingCart in ShoppingCartVM.CartList)
            {
                shoppingCart.Price = GetPriceBasedOnQuantity(shoppingCart) ;
                ShoppingCartVM.OrderHeader.OrderTotal = shoppingCart.Price * shoppingCart.Count;
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
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new ShoppingCartVM()
            {
                CartList = _unitOfWork.ShoppingCart.GetAll(filter: u => u.AppUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };

            IEnumerable<AppUser> userList = _unitOfWork.AppUser.GetAll();

            ShoppingCartVM.OrderHeader.AppUser = _unitOfWork.AppUser.Get(u => u.Id == userId);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.AppUser.Name;
            ShoppingCartVM.OrderHeader.Address = ShoppingCartVM.OrderHeader.AppUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.AppUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.AppUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.AppUser.PostalCode;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.AppUser.PhoneNumber;

            foreach (var shoppingCart in ShoppingCartVM.CartList)
            {
                shoppingCart.Price = GetPriceBasedOnQuantity(shoppingCart);
                ShoppingCartVM.OrderHeader.OrderTotal = shoppingCart.Price * shoppingCart.Count;
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.OrderHeader.AppUserId = userId;
            AppUser appUser = _unitOfWork.AppUser.Get(u=>u.Id == userId);
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;

            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.StatusPending;

            ShoppingCartVM.CartList = _unitOfWork.ShoppingCart.GetAll(u=>u.AppUserId == userId,
                includeProperties: "Product");

            // Add OrderHeader to DB
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var shoppingCart in ShoppingCartVM.CartList)
            {
                shoppingCart.Price = GetPriceBasedOnQuantity(shoppingCart);
                ShoppingCartVM.OrderHeader.OrderTotal = shoppingCart.Price * shoppingCart.Count;
                OrderDetail orderDetail = new OrderDetail()
                {
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    ProductId = shoppingCart.ProductId,
                    Count = shoppingCart.Count,
                    Price = shoppingCart.Price,
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();

            }

            return RedirectToAction(nameof(Payment), new {OrderHeaderId = ShoppingCartVM.OrderHeader.Id}); 
        }

        public IActionResult Payment(int OrderHeaderId)
        {

            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderHeaderId, includeProperties: "AppUser");


            // clear shopping cart
            IEnumerable<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.AppUserId == orderHeader.AppUserId);
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);

            _unitOfWork.Save();
            TempData["id"] = orderHeader.Id;
            return View(new {id = OrderHeaderId});
        }

        [HttpPost]
        [ActionName("PaymentPost")]
        public IActionResult PaymentPost()
        {
            int OrderHeaderId;
            if (TempData.ContainsKey("OrderHeaderId"))
            {
                OrderHeaderId = (int)TempData["OrderHeaderId"];
                OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderHeaderId, includeProperties: "AppUser");

                orderHeader.OrderStatus = SD.StatusApproved;
                orderHeader.PaymentStatus = SD.PaymentStatusApproved;
                _unitOfWork.OrderHeader.Update(orderHeader);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            // update orderHeader status to "Approved"



            return RedirectToAction(nameof(Index));
        }

        private double GetPriceBasedOnQuantity (ShoppingCart shoppingCart)
        {
            if(shoppingCart.Count < 50) return shoppingCart.Product.Price;
            else if(shoppingCart.Count < 100) return shoppingCart.Product.Price50;
            else return shoppingCart.Product.Price100;
        }
    }
}
