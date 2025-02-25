using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Domain.Entities;

namespace WhiteLagoon.Application.Common.Ultilitiy
{
    public static class StaticDetail
    {
        public const string Role_Customer = "Customer";
        public const string Role_Admin = "Admin";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusCheckedIn = "CheckedIn";
        public const string StatusCompleted = "Completed";
        public const string StatusCanceled = "Canceled";
        public const string StatusRefunded = "Refunded";

        public static int VillaRoomAvailable_Count(int villaId, 
            List<VillaNumber> villaNumberList, 
            DateOnly checkInDate, 
            int nights, 
            List<Booking> bookings)
        {
            List<int> bookingInDate = new();
            int finalAvailableRoomForAllNight = int.MaxValue;
            var roomInVilla = villaNumberList.Where(x => x.VillaId == villaId).Count(); 

            for (int i = 0; i < nights; i++)
            {
                var villasBooked = bookings.Where(b => b.CheckInDate <= checkInDate.AddDays(i) 
                && b.CheckOutDate > checkInDate.AddDays(i) 
                && b.VillaId == villaId);

                foreach (var booking in villasBooked)
                {
                    if (!bookingInDate.Contains(booking.Id))
                    {
                        bookingInDate.Add(booking.Id);
                    }
                }

                var totalAvailableRoom = roomInVilla - bookingInDate.Count();

                if (totalAvailableRoom == 0)
                {
                    return 0;
                }
                else
                {
                    if (finalAvailableRoomForAllNight > totalAvailableRoom)
                    {
                        finalAvailableRoomForAllNight = totalAvailableRoom;
                    }
                }
            }
            return finalAvailableRoomForAllNight;
        }
    }
}
