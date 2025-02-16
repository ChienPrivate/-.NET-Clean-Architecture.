using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infratructure.Data;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaNumberController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

        public VillaNumberController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var villaNumbers = await _unitOfWork.VillaNumber.GetAllAsync(includeProperties :"Villa");
            return View(villaNumbers);
        }

        public async Task<IActionResult> Create()
        {
            VillaNumberVM villaNumberVM = new()
            {
                VillaList = (await _unitOfWork.Villa.GetAllAsync())
                .Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                }),

            };

            return View(villaNumberVM);

        }

        [HttpPost]
        public async Task<IActionResult> Create(VillaNumberVM obj)
        {

            bool roomNumberExists = await _unitOfWork.VillaNumber.AnyAsync(u => u.Villa_Number == obj.VillaNumber.Villa_Number);

            if (ModelState.IsValid && !roomNumberExists)
            {
                await _unitOfWork.VillaNumber.AddAsync(obj.VillaNumber);
                await _unitOfWork.SaveAsync();
                TempData["success"] = "Villa Number has been created successfully.";
                return RedirectToAction("Index", "VillaNumber");
            }

            if (roomNumberExists)
                TempData["error"] = "The villa number already exists.";

            obj.VillaList = (await _unitOfWork.Villa.GetAllAsync())
                .Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                });

            return View(obj);
        }

        public async Task<IActionResult> Update(int villaNumber)
        {

            VillaNumberVM obj = new VillaNumberVM()
            {
                VillaNumber =  await _unitOfWork.VillaNumber.GetAsync(v => v.Villa_Number.Equals(villaNumber)) ?? new VillaNumber(),
                VillaList = (await _unitOfWork.Villa.GetAllAsync()).Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                })
            };

            if (obj.VillaNumber is null || string.IsNullOrEmpty(obj.VillaNumber.Villa_Number.ToString()))
            {
                return RedirectToAction("Error", "Home");
            }

            return View(obj);
        }

        [HttpPost]
        public async Task<IActionResult> Update(VillaNumberVM obj)
        {
            if (ModelState.IsValid) 
            {
                _unitOfWork.VillaNumber.Update(obj.VillaNumber);
                await _unitOfWork.SaveAsync();
                TempData["success"] = "Updated Villa Number successfully";
                return RedirectToAction("Index");
            }

            obj.VillaList = (await _unitOfWork.Villa.GetAllAsync()).Select(v => new SelectListItem
            {
                Text = v.Name,
                Value = v.Id.ToString()
            });

            return View(obj);
        }

        public async Task<IActionResult> Delete(int villaNumber)
        {
            VillaNumberVM obj = new VillaNumberVM()
            {
                VillaNumber = await _unitOfWork.VillaNumber.GetAsync(v => v.Villa_Number.Equals(villaNumber)) ?? new VillaNumber(),
                VillaList = (await _unitOfWork.Villa.GetAllAsync()).Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                })
            };

            if (obj is null)
            {
                return RedirectToAction("Error","Home");
            }


            return View(obj);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(VillaNumberVM obj)
        {

            var objFromDb = await _unitOfWork.VillaNumber.GetAsync(vn => vn.Villa_Number.Equals(obj.VillaNumber.Villa_Number));

            if (objFromDb is not null)
            {
                _unitOfWork.VillaNumber.Remove(objFromDb);
                await _unitOfWork.SaveAsync();
                TempData["success"] = "Villa Number Deleted successfully";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Villa Number could not be deleted.";
            return View(obj);
        }
    }
}
