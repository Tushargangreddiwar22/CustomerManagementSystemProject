using System.ComponentModel.DataAnnotations;

namespace MVCDHProject.Models
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Compare("Password",ErrorMessage ="Confirm password should not match")]
        public string ConfirmPassword { get; set; }
    }
}
