using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.DTO.Authentication
{
    public class TokenRequestVM
    {
        public string  Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
