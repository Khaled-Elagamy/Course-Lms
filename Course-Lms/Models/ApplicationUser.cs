using Microsoft.AspNetCore.Identity;

namespace Course_Lms.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}
