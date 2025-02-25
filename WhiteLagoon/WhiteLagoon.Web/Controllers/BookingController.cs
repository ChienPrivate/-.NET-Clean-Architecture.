using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Ultilitiy;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public BookingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;

            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value; // => notice this method only using when loged in or else it'll thrown an err

            ApplicationUser user = await _unitOfWork.User.GetAsync(u => u.Id == userId);

            Booking booking = new Booking()
            {
                VillaId = villaId,
                Villa = await _unitOfWork.Villa.GetAsync(v => v.Id == villaId, includeProperties: "VillaAmenity"),
                CheckInDate = checkInDate,
                Nights = nights,
                CheckOutDate = checkInDate.AddDays(nights),
                UserId = user.Id,
                Phone = user.PhoneNumber,
                Email = user.Email,
                Name = user.Name,
            };
            booking.TotalCost = booking.Villa.Price * nights;
            return View(booking);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> FinalizeBooking(Booking bookingInfo)
        {
            var villa = await _unitOfWork.Villa.GetAsync(u => u.Id == bookingInfo.VillaId);
            bookingInfo.TotalCost = villa.Price * bookingInfo.Nights;

            bookingInfo.Status = StaticDetail.StatusPending;
            bookingInfo.BookingDate = DateTime.Now;

            var villaNumberList = (await _unitOfWork.VillaNumber.GetAllAsync()).ToList();
            var bookedVillas = (await _unitOfWork.Booking.GetAllAsync(bv => bv.Status.ToLower() == StaticDetail.StatusApproved.ToLower()
            || bv.Status.ToLower() == StaticDetail.StatusCheckedIn.ToLower())).ToList();

            int roomAvailable = StaticDetail
                    .VillaRoomAvailable_Count(villa.Id, villaNumberList, bookingInfo.CheckInDate, bookingInfo.Nights, bookedVillas);

            if (roomAvailable == 0)
            {
                TempData["error"] = "Room has been sold out";

                return RedirectToAction(nameof(FinalizeBooking), new
                {
                    villaId = villa.Id,
                    checkInDate = bookingInfo.CheckInDate,
                    nights = bookingInfo.Nights,
                });
            }

            await _unitOfWork.Booking.AddAsync(bookingInfo);
            await _unitOfWork.SaveAsync();

            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={bookingInfo.Id}", // method 1 : write a direct Url and param
                CancelUrl = domain + Url.Action(nameof(FinalizeBooking), 
                ControllerContext.ActionDescriptor.ControllerName, 
                new { 
                    villaId = bookingInfo.VillaId, 
                    checkInDate = bookingInfo.CheckInDate, 
                    nights = bookingInfo.Nights
                }), // method 2: define the Url by code
            };

            options.LineItems.Add(new SessionLineItemOptions
            {

                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(bookingInfo.TotalCost * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name,
                        Description = villa.Description,
                        //Images = new List<string> { domain + villa.ImageUrl },
                    },
                },
                Quantity = 1,
            });

            var service = new SessionService();
            Session session = service.Create(options);

            await _unitOfWork.Booking.UpdateStripePaymentIdAsync(bookingInfo.Id, session.Id, session.PaymentIntentId);
            await _unitOfWork.SaveAsync();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [Authorize]
        public async Task<IActionResult> BookingConfirmation(int bookingId)
        {
            Booking bookingFromDb = await _unitOfWork.Booking.GetAsync(u => u.Id == bookingId,
                includeProperties: "User,Villa");

            if (bookingFromDb.Status == StaticDetail.StatusPending)
            {
                var service = new SessionService();
                Session session = service.Get(bookingFromDb.StripeSessionId);

                if (session.PaymentStatus == "paid")
                {
                    await _unitOfWork.Booking.UpdateStatusAsync(bookingFromDb.Id,StaticDetail.StatusApproved);
                    await _unitOfWork.Booking.UpdateStripePaymentIdAsync(bookingFromDb.Id,session.Id,session.PaymentIntentId);
                    await _unitOfWork.SaveAsync();
                }
            }

            return View(bookingId);
        }

        [Authorize]
        public async Task<IActionResult> BookingDetails(int bookingId)
        {
            Booking? bookingFromDb = await _unitOfWork.Booking.GetAsync(b => b.Id == bookingId,includeProperties: "User,Villa");

            if (bookingFromDb.VillaNumber == 0 && bookingFromDb.Status == StaticDetail.StatusApproved)
            {
                var availableVillaNumber = await AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId);

                bookingFromDb.VillaNumbers = (await _unitOfWork.VillaNumber.GetAllAsync(vn => vn.VillaId == bookingFromDb.VillaId
                && availableVillaNumber.Any(avn => avn == vn.Villa_Number))).ToList();
            }

            return View(bookingFromDb);
        }
        
        [Authorize(Roles = StaticDetail.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> CheckIn(Booking booking)
        {
            if (!int.TryParse(booking.VillaNumber.ToString(), out _)) // check if this a number
            {

            }

            await _unitOfWork.Booking.UpdateStatusAsync(booking.Id, StaticDetail.StatusCheckedIn, booking.VillaNumber);
            await _unitOfWork.SaveAsync();
            TempData["success"] = "Booking updated successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [Authorize(Roles = StaticDetail.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> CheckOut(Booking booking)
        {
            await _unitOfWork.Booking.UpdateStatusAsync(booking.Id, StaticDetail.StatusCompleted, booking.VillaNumber);
            await _unitOfWork.SaveAsync();
            TempData["success"] = "Booking completed successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [Authorize(Roles = StaticDetail.Role_Admin)]
        [HttpPost]
        public async Task<IActionResult> CancelBooking(Booking booking)
        {
            await _unitOfWork.Booking.UpdateStatusAsync(booking.Id, StaticDetail.StatusCanceled, 0);
            await _unitOfWork.SaveAsync();
            TempData["success"] = "Booking canceled successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        private async Task<List<int>> AssignAvailableVillaNumberByVilla(int villaId)
        {
            List<int> availableVillaNumbers = new();

            var villaNumbers = await _unitOfWork.VillaNumber.GetAllAsync(v => v.VillaId == villaId);

            var checkedInVilla = (await _unitOfWork.Booking.GetAllAsync(u => u.VillaId == villaId && u.Status == StaticDetail.StatusCheckedIn))
                .Select(u => u.VillaNumber)
                .ToList();

            foreach (var villaNumber in villaNumbers)
            {
                if (!checkedInVilla.Contains(villaNumber.Villa_Number))
                {
                    availableVillaNumbers.Add(villaNumber.Villa_Number);
                }
            }

            return availableVillaNumbers;

        }

        #region API Calls
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllBookingsData(string status)
        {
            IEnumerable<Booking> objBooking;
            if (User.IsInRole(StaticDetail.Role_Admin))
            {
                // method 1
                if (!string.IsNullOrEmpty(status))
                {
                    objBooking = await _unitOfWork.Booking.GetAllAsync(b => b.Status.ToLower().Equals(status.ToLower()), includeProperties: "User,Villa");
                }
                else
                {
                    objBooking = await _unitOfWork.Booking.GetAllAsync(includeProperties: "User,Villa");
                }
            }
            else
            {
                var claimIdentity = (ClaimsIdentity?)User.Identity ?? new ClaimsIdentity();
                var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                // method 1
                if (!string.IsNullOrEmpty(status))
                {
                    objBooking = await _unitOfWork.Booking.GetAllAsync(b => b.UserId == userId && b.Status.ToLower().Equals(status.ToLower()), includeProperties: "User,Villa"); 
                }
                else
                {
                    objBooking = await _unitOfWork.Booking.GetAllAsync(b => b.UserId == userId, includeProperties: "User,Villa");
                }
                
            }
            // method 2
            /*if (!string.IsNullOrEmpty(status))
            {
                objBooking = objBooking.Where(b => b.Status?.ToLower() == status.ToLower() );
            }*/
            return Json(new { data = objBooking });
        }
        #endregion

    }
}
