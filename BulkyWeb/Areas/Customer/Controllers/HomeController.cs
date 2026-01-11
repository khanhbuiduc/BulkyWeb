using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWorkRepository _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWorkRepository unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category");
            return View(productList);
        }

        public IActionResult Details(int id)
        {
            ShoppingCart cart = new()
            {
                Product = _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "Category"),
                Count = 1,
                ProductId = id
            };
            
            if (cart.Product == null)
            {
                return NotFound();
            }
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userId && 
                u.ProductId == shoppingCart.ProductId);

            if (cartFromDb != null)
            {
                // Shopping cart exists, update count
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            else
            {
                // Add cart record - reset Id to 0 to let database generate it
                shoppingCart.Id = 0;
                _unitOfWork.ShoppingCart.Add(shoppingCart);
            }
            
            _unitOfWork.Save();
            
            // Update session with cart count
            HttpContext.Session.SetInt32(SD.SessionCart, 
                _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count());
            
            TempData["success"] = "Cart updated successfully";
            
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
