using System;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Core.Services
{
    public class UserFactory
    {
        public static User CreateUser(string userType, string username, string email, string password)
        {
            return userType.ToLower() switch
            {
                "customer" => new Customer
                {
                    Username = username,
                    Email = email,
                    Password = password
                },
                "organizer" => new Organizer
                {
                    Username = username,
                    Email = email,
                    Password = password
                },
                "administrator" => new Administrator
                {
                    Username = username,
                    Email = email,
                    Password = password,
                    Role = "Admin"
                },
                _ => throw new ArgumentException($"Invalid user type: {userType}")
            };
        }
    }
} 