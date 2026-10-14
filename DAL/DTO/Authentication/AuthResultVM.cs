using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTO.Authentication
{
    public class AuthResultVM
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
