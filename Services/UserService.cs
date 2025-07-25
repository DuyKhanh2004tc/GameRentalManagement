using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GameRentalManagement.Models;
using GameRentalManagement.Utils;

namespace GameRentalManagement.Services
{
    public class UserService
    {
        private readonly GameRentalDbContext _context;

        public UserService(GameRentalDbContext context)
        {
            _context = context;
        }

        public bool AddUser(string username, string password, string role, out string message)
        {
            if (_context.Users.Any(u => u.Username == username))
            {
                message = "Username already exists.";
                return false;
            }

            string pattern = @"^(?=.*[A-Z])(?=.*\d).{6,}$";
            if (!Regex.IsMatch(password, pattern))
            {
                message = "Password must be at least 6 characters and include at least 1 uppercase letter and 1 number.";
                return false;
            }

            User newUser = new()
            {
                Username = username,
                Password = PasswordEncryption.HashPassword(password),
                Role = role
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            message = "User added successfully.";
            return true;
        }
    }
}
