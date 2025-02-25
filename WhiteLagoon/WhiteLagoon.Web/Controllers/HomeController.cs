using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Ultilitiy;
using WhiteLagoon.Web.Models;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            HomeVM vm = new HomeVM()
            {
                VillaList = await _unitOfWork.Villa.GetAllAsync(includeProperties: "VillaAmenity"),
                Nights = 1,
                CheckInDate = DateOnly.FromDateTime(DateTime.Now)
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> GetVillasByDate(int nights, DateOnly checkInDate)
        {
            /*var villaList = await _unitOfWork.Villa.GetAllAsync(includeProperties: "VillaAmenity");
            foreach (var villa in villaList)
            {
                if (villa.Id % 2 == 0)
                {
                    villa.IsAvailable = false;
                }
            }  this code is obsoleted -- it might be terminated */

            /*await Task.Delay(2000);*/ /* this syntax equal to: Thread.Sleep(2000);*/

            var villaList = (await _unitOfWork.Villa.GetAllAsync(includeProperties: "VillaAmenity")).ToList();

            var villaNumberList = (await _unitOfWork.VillaNumber.GetAllAsync()).ToList();

            var bookedVillas = (await _unitOfWork.Booking.GetAllAsync(bv => bv.Status.ToLower() == StaticDetail.StatusApproved.ToLower()
            || bv.Status.ToLower() == StaticDetail.StatusCheckedIn.ToLower())).ToList();

            foreach (var villa in villaList)
            {
                int roomAvailable = StaticDetail
                    .VillaRoomAvailable_Count(villa.Id, villaNumberList, checkInDate, nights, bookedVillas);

                villa.IsAvailable = roomAvailable > 0 ? true : false;
            }


            HomeVM homeVM = new HomeVM()
            {
                CheckInDate = checkInDate,
                VillaList = villaList,
                Nights = nights
            };

            return PartialView("_VillaList",homeVM);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            /*return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });*/
            return View();
        }   

    }
}
