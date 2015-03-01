using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace account.ViewModel
{
    public class RegisteredUser
    {
        [Required]
        [Display(Name = "User name")]
        [StringLength(16)]
        public string UserName { get; set; }

        [Required]
        [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$",
        ErrorMessage = "This is not a valid email address.")]
        [Display(Name = "Email")]
        [StringLength(50)]
        public string Email { get; set; }

        [Display(Name = "Street Address")]
        [StringLength(50)]
        public string Address { get; set; }

        [Display(Name = "Phone")]
        [RegularExpression(@"^\(?\d{3}\)?-? *\d{3}-? *-?\d{4}$",
        ErrorMessage = "This is not a valid phone number.")]
        public string Phone { get; set; }

        [Display(Name = "City")]
        [StringLength(15)]

        public string City { get; set; }

        [Display(Name = "State / Province")]
        [StringLength(15)]
        public string Province { get; set; }


        [Display(Name = "Country")]
        [StringLength(15)]
        public string Country { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "Password has minimum length of 8, maximum of 30")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password Confirm")]
        [Compare("Password", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

    }
}