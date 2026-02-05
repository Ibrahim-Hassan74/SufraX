using EStoreX.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Account.Requests
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "{0} can't be blank")]
        [Display(Name = "First Name")]
        public string? UserName { get; set; }
        [Required(ErrorMessage = "{0} can't be blank")]
        [EmailAddress(ErrorMessage = "{0} should be in a proper email address format")]
        [Remote(action: "IsEmailAlreadyRegistered", controller: "Account", ErrorMessage = "Email is already use")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "{0} can't be blank")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "{0} should contains numbers only")]
        public string? Phone { get; set; }
        [Required(ErrorMessage = "{0} can't be blank")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "{0} should be at least 7 characters")]
        public string? Password { get; set; }
        [Required(ErrorMessage = "{0} can't be blank")]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }
    }
}
