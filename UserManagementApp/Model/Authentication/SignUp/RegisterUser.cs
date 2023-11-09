using System.ComponentModel.DataAnnotations;

namespace UserManagementApp.Model.Authentication.SignUp
{
    public class RegisterUser
    {
        [Required(ErrorMessage = "Name is required")]
        public string? UserName { get; set; }
        [Required(ErrorMessage = "Name is Email")]
        [EmailAddress]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Name is Password")]
        public string? Password { get; set; }
        public string[] UserRoles { get; set; }
    }
}
