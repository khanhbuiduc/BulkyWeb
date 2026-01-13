using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWorkRepository _unitOfWork;

        public OrderController(IUnitOfWorkRepository unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var orderHeader = _unitOfWork.OrderHeader.Get(
                u => u.Id == orderId && u.ApplicationUserId == userId, 
                includeProperties: "ApplicationUser"
            );

            if (orderHeader == null)
            {
                return NotFound();
            }

            OrderVM orderVM = new()
            {
                OrderHeader = orderHeader,
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };

            return View(orderVM);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            IEnumerable<OrderHeader> objOrderHeaders = _unitOfWork.OrderHeader
                .GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");

            switch (status)
            {
                case "pending":
                    // Pending bao g?m: OrderStatus Pending HO?C PaymentStatus DelayedPayment
                    objOrderHeaders = objOrderHeaders.Where(u => 
                        u.OrderStatus == SD.StatusPending || 
                        u.PaymentStatus == SD.PaymentStatusDelayedPayment ||
                        u.PaymentStatus == SD.PaymentStatusPending);
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }

            return Json(new { data = objOrderHeaders });
        }

        #endregion
    }
}
