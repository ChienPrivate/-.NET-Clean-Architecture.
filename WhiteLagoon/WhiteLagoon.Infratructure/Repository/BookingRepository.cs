using Microsoft.EntityFrameworkCore;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Ultilitiy;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infratructure.Data;

namespace WhiteLagoon.Infratructure.Repository
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        private readonly ApplicationDbContext _db;
        public BookingRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Booking entity)
        {
            _db.Bookings.Update(entity);
        }

        public async Task UpdateStatusAsync(int bookingId, string bookingStatus, int villaNumber = 0)
        {
            var bookingFromDb = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (bookingFromDb is not null)
            {
                bookingFromDb.Status = bookingStatus;

                if (bookingStatus == StaticDetail.StatusCheckedIn)
                {
                    bookingFromDb.VillaNumber = villaNumber;
                    bookingFromDb.ActualCheckInDate = DateTime.Now;
                }

                if (bookingStatus == StaticDetail.StatusCompleted)
                {
                    bookingFromDb.ActualCheckOutDate = DateTime.Now;
                }
            }
        }

        public async Task UpdateStripePaymentIdAsync(int bookingId, string sessionId, string paymentIntentId)
        {
            var bookingFromDb = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (bookingFromDb is not null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    bookingFromDb.StripeSessionId = sessionId;
                }
                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    bookingFromDb.StripePaymentIntentId = paymentIntentId;
                    bookingFromDb.PaymentDate = DateTime.Now;
                    bookingFromDb.IsPaymentSuccessful = true;
                }
            }
        }
    }
}
