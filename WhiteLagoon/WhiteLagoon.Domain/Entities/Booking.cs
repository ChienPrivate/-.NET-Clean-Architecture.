using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhiteLagoon.Domain.Entities
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        [Required]
        public int VillaId { get; set; }
        [ForeignKey(nameof(VillaId))]
        public Villa Villa { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        public string Phone { get; set; }

        [Required]
        public double TotalCost { get; set; }
        [Display(Name = "No. of nights")]
        public int Nights { get; set; }
        public string? Status { get; set; }

        [Required]
        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; }
        [Required]
        [Display(Name = "Check-in Date")]
        public DateOnly CheckInDate { get; set; }
        [Required]
        [Display(Name = "Check-out Date")]
        public DateOnly CheckOutDate { get; set; }

        public bool IsPaymentSuccessful { get; set; }
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; }

        [Display(Name = "Stripe Session Id")]
        public string? StripeSessionId { get; set; }
        [Display(Name = "Stripe PaymentIntent Id")]
        public string? StripePaymentIntentId { get; set; }
        [Display(Name = "Actual Check-in Date")]
        public DateTime ActualCheckInDate { get; set; }
        [Display(Name = "Actual Check-out Date")]
        public DateTime ActualCheckOutDate { get; set; }
        [Display(Name = "Villa Number")]
        public int VillaNumber { get; set; }
        [NotMapped]
        [Display(Name = "Villa Numbers List")]
        public List<VillaNumber> VillaNumbers { get; set; }
    }
}
