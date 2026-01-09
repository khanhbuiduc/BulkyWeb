using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWorkRepository _unitOfWork;
        
        public CompanyController(IUnitOfWorkRepository unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetAll()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = objCompanyList });
        }

        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                // Create
                return View(new Company());
            }
            else
            {
                // Update
                Company companyObj = _unitOfWork.Company.Get(u => u.Id == id);
                if (companyObj == null)
                {
                    return NotFound();
                }
                return View(companyObj);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company companyObj)
        {
            if (ModelState.IsValid)
            {
                if (companyObj.Id == 0)
                {
                    // Create
                    _unitOfWork.Company.Add(companyObj);
                    TempData["success"] = "Company created successfully";
                }
                else
                {
                    // Update
                    _unitOfWork.Company.Update(companyObj);
                    TempData["success"] = "Company updated successfully";
                }
                
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Failed to save company. Please check all fields.";
            }
            
            return View(companyObj);
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var companyToBeDeleted = _unitOfWork.Company.Get(u => u.Id == id);
            if (companyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.Company.Remove(companyToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }
    }
}
