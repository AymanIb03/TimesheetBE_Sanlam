using Microsoft.AspNetCore.Identity;

namespace Timesheet.Models
{
    public class AppUser : IdentityUser
    {
       public bool IsActive { get; set; } = true;

    }
}
