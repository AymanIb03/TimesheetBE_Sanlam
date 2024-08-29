    using System.ComponentModel.DataAnnotations;

    namespace Timesheet.Data.Models
    {
        public class DtoNewUser
        {
            [Required]
            public string userName { get; set; }

            [Required]
            public string email { get; set; }
            [Required]
            public string role { get; set; }
        }
    }
