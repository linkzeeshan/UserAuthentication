namespace UserManagementApp.Model
{
    using Microsoft.AspNetCore.Identity;

    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        // Add custom properties as needed
    }

    public class ApplicationRole : IdentityRole
    {
        // Add custom properties as needed
    }
}
