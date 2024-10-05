using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Collections;
using System.Security.Claims;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        IUnitOfWork _unitOfWork;
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAll(string status) {

            //return Json(new { data = objOrderHeaders });

            //IEnumerable<OrderHeader> objOrderHeaders;
            IEnumerable<OrderHeader> objOrderHeaders;
            //string status = "all";
            objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "AppUser").ToList();

            switch (status)
            {
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;

                case "pending":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusPending);
                    break;

                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;

                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;

                default:
                    break;
            }

            return Json(new { data = objOrderHeaders });
        }

        public IActionResult Details(int orderId) {

            IEnumerable<OrderDetail> detailList = _unitOfWork.OrderDetail.GetAll(u=>u.OrderHeaderId == orderId, includeProperties:"Product");

            OrderVM orderVM = new OrderVM()
            {
                Details = detailList,
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId)
            };

            return View(orderVM);
        }

    }
}
