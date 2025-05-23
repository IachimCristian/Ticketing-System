using System;
using TicketingSystem.Core.Entities;
using BCrypt.Net;

namespace TicketingSystem.Core.Services
{
    public class UserFactory
    {
        public static User CreateUser(string userType, string username, string email, string password)
        {
            // Hash the password using BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            
            return userType.ToLower() switch
            {
                "customer" => new Customer
                {
                    Username = username,
                    Email = email,
                    Password = hashedPassword
                },
                "organizer" => new Organizer
                {
                    Username = username,
                    Email = email,
                    Password = hashedPassword
                },
                "administrator" => new Administrator
                {
                    Username = username,
                    Email = email,
                    Password = hashedPassword,
                    Role = "Admin"
                },
                _ => throw new ArgumentException($"Invalid user type: {userType}")
            };
        }
    }
} 