using System.ComponentModel.DataAnnotations;

namespace Timesheet.Data.Models
{
    public class DtoLogin
    {
        [Required]
        public string userName { get; set; }

        [Required]
        public string password { get; set; }
    }
}
