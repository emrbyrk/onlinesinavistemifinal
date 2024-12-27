using Microsoft.AspNetCore.Identity;

namespace onlinesinavsistemifinal.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Role { get; set; } = "Öğrenci"; // "Öğrenci" veya "Öğretmen"
    }

}
