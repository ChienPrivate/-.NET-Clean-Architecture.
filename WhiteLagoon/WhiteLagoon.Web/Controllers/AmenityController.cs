using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Ultilitiy;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    [Authorize(Roles = StaticDetail.Role_Admin)]
    public class AmenityController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public AmenityController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Index()
        {
            var objList = await _unitOfWork.Amenity.GetAllAsync(includeProperties: "Villa");
            return View(objList);
        }

        public async Task<IActionResult> Create()
        {
            AmenityVM amenityVM = new()
            {
                VillaList = (await _unitOfWork.Villa.GetAllAsync()).Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                })
            };

            return View(amenityVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AmenityVM obj)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.Amenity.AddAsync(obj.Amenity);
                await _unitOfWork.SaveAsync();
                TempData["success"] = "Amenity has been created successfully";
                return RedirectToAction("index");
            }

            obj.VillaList = (await _unitOfWork.Villa.GetAllAsync()).Select(v => new SelectListItem
            {
                Text = v.Name,
                Value = v.Id.ToString()
            });

            return View(obj);
        }

        public async Task<IActionResult> Update(int id)
        {
            AmenityVM amenityVM = new()
            {
                Amenity = await _unitOfWork.Amenity.GetAsync(a => a.Id.Equals(id)),
                VillaList = (await _unitOfWork.Villa.GetAllAsync()).Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                })
            };

            if (amenityVM.Amenity is null)
            {
                return RedirectToAction("Error","Home");
            }

            return View(amenityVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(AmenityVM amenityVM)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Amenity.Update(amenityVM.Amenity);
                await _unitOfWork.SaveAsync();
                TempData["success"] = "Amenity has been update succesfully";
                return RedirectToAction("Index");
            }

            amenityVM.VillaList = (await _unitOfWork.Villa.GetAllAsync()).Select(v => new SelectListItem
            {
                Text = v.Name,
                Value = v.Id.ToString()
            });

            return View(amenityVM);
        }

        public async Task<IActionResult> Delete(int id)
        {
            AmenityVM amenityVM = new()
            {
                Amenity = await _unitOfWork.Amenity.GetAsync(a => a.Id.Equals(id)), // this expression is similar to => _unitOfWork.Amenity.GetAsync(a => a.Id == id);
                VillaList = (await _unitOfWork.Villa.GetAllAsync()).Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                })
            };

            if (amenityVM.Amenity is null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(amenityVM);

        }

        [HttpPost]
        public async Task<IActionResult> Delete(AmenityVM amenityVM)
        {
            Amenity? objFromDb = await _unitOfWork.Amenity.GetAsync(a => a.Id.Equals(amenityVM.Amenity.Id));

            if (objFromDb is not null)
            {
                _unitOfWork.Amenity.Remove(objFromDb);
                await _unitOfWork.SaveAsync();
                TempData["success"] = "Amenity has been Removed successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["erorr"] = "The villa number could not be deleted.";
            return View(amenityVM);
        }
    }
}
