using System.ComponentModel.DataAnnotations;

namespace UserManagementApp.Model.Authentication.Login
{
    public class ResetPassword
    {
        public string? Password { get; set; }
        [Compare("Password", ErrorMessage ="The password and confirmation password are not match")]
        public string? ConfirmPassword { get; set; }
        public string? Token { get; set; }
        public string? Email { get; set; }
    }
}
