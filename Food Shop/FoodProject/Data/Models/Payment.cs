using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodProject.Data.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        [Required(ErrorMessage = "Please enter your name and surname as written on the card.")]
        public string NameSurname { get; set; }

        [Required(ErrorMessage = "Email field cannot be empty.")]
        public string Email { get; set; }

        [StringLength(11, ErrorMessage = "Phone number must be 11 characters.")]
        [Required(ErrorMessage = "Phone number field cannot be empty.")]
        public string MobileNumber { get; set; }

        [Required(ErrorMessage = "City field cannot be empty.")]
        public string City { get; set; }

        [Required(ErrorMessage = "Address field cannot be empty.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Card number cannot be empty.")]
        [NotMapped]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Please enter the card's expiration date (Month/Year).")]
        [NotMapped]
        public string CardMonth_Year { get; set; }

        [Required(ErrorMessage = "Please enter the CVC number on the back of the card.")]
        [NotMapped]
        public string CardCVC { get; set; }

        public double ShoppingTotal { get; set; }

        public int AppUserID { get; set; }
        public virtual AppUser AppUser { get; set; }

        public List<Order> Orders { get; set; }
    }
}