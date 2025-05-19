using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Core.Services
{
    public interface IAuthenticationService
    {
        Task<User> AuthenticateAsync(string email, string password);
        string GetUserType(User user);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository<Customer> _customerRepository;
        private readonly IUserRepository<Organizer> _organizerRepository;
        private readonly IUserRepository<Administrator> _adminRepository;

        public AuthenticationService(
            IUserRepository<Customer> customerRepository,
            IUserRepository<Organizer> organizerRepository,
            IUserRepository<Administrator> adminRepository)
        {
            _customerRepository = customerRepository;
            _organizerRepository = organizerRepository;
            _adminRepository = adminRepository;
        }

        public async Task<User> AuthenticateAsync(string email, string password)
        {
            // Try to authenticate as an administrator
            var admin = await _adminRepository.GetByEmailAsync(email);
            if (admin != null && admin.Password == password && admin.IsActive)
            {
                return admin;
            }

            // Try to authenticate as an organizer
            var organizer = await _organizerRepository.GetByEmailAsync(email);
            if (organizer != null && organizer.Password == password)
            {
                return organizer;
            }

            // Try to authenticate as a customer
            var customer = await _customerRepository.GetByEmailAsync(email);
            if (customer != null && customer.Password == password)
            {
                return customer;
            }

            return null;
        }

        public string GetUserType(User user)
        {
            return user switch
            {
                Administrator => "Admin",
                Organizer => "Organizer",
                Customer => "Customer",
                _ => throw new ArgumentException("Unknown user type")
            };
        }
    }
} 