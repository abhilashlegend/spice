using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Models
{
    public class ApplicationUser: Microsoft.AspNetCore.Identity.IdentityUser
    {
        [Required]
        public string Name { get; set; }

        public string SteetAddress { get; set; }

        public string City { get; set; }

        public string State { get; set; }
    }
}
