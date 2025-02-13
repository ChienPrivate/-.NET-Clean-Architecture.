using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infratructure.Data;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly ApplicationDbContext _db;
        public VillaController(ApplicationDbContext db)
        {
            _db = db;
        }
        // Gemini API key AIzaSyCanNWt_0wDfApizWajkwx0_TLgA5QWxyE
        public async Task<IActionResult> Index()
        {
            var villa = await _db.Villas.ToListAsync();

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
                obj.CreatedDate = DateTime.Now;
                await _db.Villas.AddAsync(obj);
                await _db.SaveChangesAsync();
                TempData["success"] = "Created successfully";
                return RedirectToAction(nameof(Index), "Villa");
            }
            return View();
        }

        public async Task<IActionResult> Update(int id)
        {
            var obj = await _db.Villas.FirstOrDefaultAsync(v => v.Id.Equals(id));

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
                obj.UpdatedDate = DateTime.Now;
                _db.Villas.Update(obj);
                await _db.SaveChangesAsync();
                TempData["success"] = "Updated successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public async Task<IActionResult> Delete(int id)
        {
            var obj = await _db.Villas.FirstOrDefaultAsync(v => v.Id == id);

            if (obj is null)
            {
                return View("Error","Home");
            }

            return View(obj);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Villa obj)
        {
            if (obj is not null || obj.Id is not 0)
            {
                _db.Villas.Remove(obj);
                await _db.SaveChangesAsync();
                TempData["success"] = "Deleted successfully";
                return RedirectToAction("Index");
            }

            return View(obj);
        }
    }
}
