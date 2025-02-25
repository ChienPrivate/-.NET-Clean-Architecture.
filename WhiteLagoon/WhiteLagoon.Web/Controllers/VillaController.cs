using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Ultilitiy;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infratructure.Data;

namespace WhiteLagoon.Web.Controllers
{
    [Authorize(Roles = StaticDetail.Role_Admin)]
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VillaController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            var villa = await _unitOfWork.Villa.GetAllAsync();

            return View(villa);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Villa obj)
        {
            if (obj.Name == obj.Description)
            {
                ModelState.AddModelError("", "The description cannot exactly match the name");
            }

            if (ModelState.IsValid)
            {
                if (obj.Image is not null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(obj.Image.FileName);
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\VillaImages");

                    using (var filestream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create))
                        obj.Image.CopyTo(filestream);

                    obj.ImageUrl = @"\images\VillaImages\" + fileName;
                }
                else
                {
                    obj.ImageUrl = "http://placeholder.co/600x400";
                }

                obj.CreatedDate = DateTime.Now;
                await _unitOfWork.Villa.AddAsync(obj);
                await _unitOfWork.SaveAsync();
                TempData["success"] = "Created successfully";
                return RedirectToAction(nameof(Index), "Villa");
            }
            return View();
        }

        public async Task<IActionResult> Update(int id)
        {
            var obj = await _unitOfWork.Villa.GetAsync(v => v.Id.Equals(id));

            if (obj == null) 
            {
                return RedirectToAction("Error","Home");
            }

            return View(obj);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Villa obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Image is not null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(obj.Image.FileName);
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\VillaImages");

                    if(!string.IsNullOrEmpty(obj.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }    

                    using (var filestream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create))
                        obj.Image.CopyTo(filestream);

                    obj.ImageUrl = @"\images\VillaImages\" + fileName;
                }

                obj.UpdatedDate = DateTime.Now;
                _unitOfWork.Villa.Update(obj);
                await _unitOfWork.SaveAsync();
                TempData["success"] = "Villa has been updated successfully.";
                return RedirectToAction("Index");
            }
            return View();
        }

        public async Task<IActionResult> Delete(int id)
        {
            var obj = await _unitOfWork.Villa.GetAsync(v => v.Id.Equals(id));

            if (obj is null)
            {
                return View("Error","Home");
            }

            return View(obj);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Villa obj)
        {
            Villa? objFromDb = await _unitOfWork.Villa.GetAsync(v => v.Id == obj.Id);
            if (objFromDb is not null || obj.Id > 0)
            {
                if (!string.IsNullOrEmpty(objFromDb.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, objFromDb.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                _unitOfWork.Villa.Remove(objFromDb);
                await _unitOfWork.SaveAsync();
                TempData["success"] = "Villa has been deleted successfully.";
                return RedirectToAction("Index");
            }
            TempData["error"] = "villa might not exist, please check it again";
            return View(obj);
        }
    }
}
