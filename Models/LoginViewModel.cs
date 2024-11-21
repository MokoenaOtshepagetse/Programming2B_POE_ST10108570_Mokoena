using System;

namespace Claimed.Models
{
    public class LoginViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsAdminLogin { get; set; }
    }
}