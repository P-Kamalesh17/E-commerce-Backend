using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL.DTO.Authentication
{
    public class LoginVM
    {
        [Required(ErrorMessage ="Email is required")]
        public string  Email { get; set; }
        [Required(ErrorMessage ="Password is required")]
        public string Password { get; set; }
    }
}
