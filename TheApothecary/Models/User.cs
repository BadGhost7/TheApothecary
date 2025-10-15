using System;

namespace TheApothecary.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
    }

    public enum UserRole
    {
        Customer,
        Employee,
        Admin
    }
}