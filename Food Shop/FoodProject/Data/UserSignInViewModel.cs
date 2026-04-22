using System.ComponentModel.DataAnnotations;

namespace FoodProject.Data
{
    public class UserSignInViewModel
    {
        [Required(ErrorMessage = "Please enter your username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Please enter your password")]
        public string Password { get; set; }
    }
}