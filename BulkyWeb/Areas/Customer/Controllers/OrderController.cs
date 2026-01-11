using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
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
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            IEnumerable<OrderHeader> orderHeaders = _unitOfWork.OrderHeader
                .GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");

            return View(orderHeaders);
        }

        public IActionResult Details(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader
                .Get(u => u.Id == id, includeProperties: "ApplicationUser");

            if (orderHeader == null)
            {
                return NotFound();
            }

            IEnumerable<OrderDetail> orderDetails = _unitOfWork.OrderDetail
                .GetAll(u => u.OrderHeaderId == id, includeProperties: "Product");

            ViewBag.OrderDetails = orderDetails;

            return View(orderHeader);
        }
    }
}
