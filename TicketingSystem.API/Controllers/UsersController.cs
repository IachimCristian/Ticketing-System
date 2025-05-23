using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Services;
using BCrypt.Net;

namespace TicketingSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository<Customer> _customerRepository;
        private readonly IUserRepository<Organizer> _organizerRepository;
        private readonly IUserRepository<Administrator> _adminRepository;

        public UsersController(
            IUserRepository<Customer> customerRepository, 
            IUserRepository<Organizer> organizerRepository, 
            IUserRepository<Administrator> adminRepository)
        {
            _customerRepository = customerRepository;
            _organizerRepository = organizerRepository;
            _adminRepository = adminRepository;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // First try to find the user as a customer
            var customer = await _customerRepository.GetByEmailAsync(loginDto.Email);
            if (customer != null && VerifyPassword(loginDto.Password, customer.Password))
            {
                return Ok(new { 
                    Id = customer.Id,
                    Username = customer.Username,
                    Email = customer.Email,
                    UserType = "Customer",
                    Message = "Login successful"
                });
            }

            // Try to find the user as an organizer
            var organizer = await _organizerRepository.GetByEmailAsync(loginDto.Email);
            if (organizer != null && VerifyPassword(loginDto.Password, organizer.Password))
            {
                return Ok(new { 
                    Id = organizer.Id,
                    Username = organizer.Username,
                    Email = organizer.Email,
                    UserType = "Organizer",
                    Message = "Login successful"
                });
            }

            // Try to find the user as an administrator
            var admin = await _adminRepository.GetByEmailAsync(loginDto.Email);
            if (admin != null && VerifyPassword(loginDto.Password, admin.Password))
            {
                return Ok(new { 
                    Id = admin.Id,
                    Username = admin.Username,
                    Email = admin.Email,
                    UserType = "Administrator",
                    Message = "Login successful"
                });
            }

            // If we get here, login failed
            return Unauthorized(new { Message = "Invalid email or password" });
        }

        // Helper method to verify passwords
        private bool VerifyPassword(string providedPassword, string storedPassword)
        {
            // For backward compatibility with non-hashed passwords (like admin account)
            if (!storedPassword.StartsWith("$2a$") && !storedPassword.StartsWith("$2b$") && !storedPassword.StartsWith("$2y$"))
            {
                return providedPassword == storedPassword;
            }
            
            // For BCrypt hashed passwords
            try
            {
                return BCrypt.Net.BCrypt.Verify(providedPassword, storedPassword);
            }
            catch
            {
                // If the hash is invalid, fail safely
                return false;
            }
        }

        [HttpPost("customers")]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerDto dto)
        {
            // Check if email and username are unique
            if (!await _customerRepository.IsEmailUniqueAsync(dto.Email))
            {
                return BadRequest("Email is already in use");
            }

            if (!await _customerRepository.IsUsernameUniqueAsync(dto.Username))
            {
                return BadRequest("Username is already in use");
            }

            // Create the customer using the factory
            var customer = (Customer)UserFactory.CreateUser("customer", dto.Username, dto.Email, dto.Password);
            customer.Phone = dto.Phone;
            customer.Address = dto.Address;

            await _customerRepository.AddAsync(customer);
            await _customerRepository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
        }

        [HttpGet("customers/{id}")]
        public async Task<IActionResult> GetCustomer(Guid id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        [HttpPost("organizers")]
        public async Task<IActionResult> CreateOrganizer([FromBody] OrganizerDto dto)
        {
            // Check if email and username are unique
            if (!await _organizerRepository.IsEmailUniqueAsync(dto.Email))
            {
                return BadRequest("Email is already in use");
            }

            if (!await _organizerRepository.IsUsernameUniqueAsync(dto.Username))
            {
                return BadRequest("Username is already in use");
            }

            // Create the organizer using the factory
            var organizer = (Organizer)UserFactory.CreateUser("organizer", dto.Username, dto.Email, dto.Password);
            organizer.OrganizationName = dto.OrganizationName;
            organizer.Description = dto.Description;
            organizer.ContactPhone = dto.ContactPhone;

            await _organizerRepository.AddAsync(organizer);
            await _organizerRepository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrganizer), new { id = organizer.Id }, organizer);
        }

        [HttpGet("organizers/{id}")]
        public async Task<IActionResult> GetOrganizer(Guid id)
        {
            var organizer = await _organizerRepository.GetByIdAsync(id);
            if (organizer == null)
            {
                return NotFound();
            }

            return Ok(organizer);
        }

        [HttpPost("administrators")]
        public async Task<IActionResult> CreateAdministrator([FromBody] AdminDto dto)
        {
            // Check if email and username are unique
            if (!await _adminRepository.IsEmailUniqueAsync(dto.Email))
            {
                return BadRequest("Email is already in use");
            }

            if (!await _adminRepository.IsUsernameUniqueAsync(dto.Username))
            {
                return BadRequest("Username is already in use");
            }

            // Create the administrator using the factory
            var admin = (Administrator)UserFactory.CreateUser("administrator", dto.Username, dto.Email, dto.Password);
            admin.Role = dto.Role;

            await _adminRepository.AddAsync(admin);
            await _adminRepository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAdministrator), new { id = admin.Id }, admin);
        }

        [HttpGet("administrators/{id}")]
        public async Task<IActionResult> GetAdministrator(Guid id)
        {
            var admin = await _adminRepository.GetByIdAsync(id);
            if (admin == null)
            {
                return NotFound();
            }

            return Ok(admin);
        }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class CustomerDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    public class OrganizerDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string OrganizationName { get; set; }
        public string Description { get; set; }
        public string ContactPhone { get; set; }
    }

    public class AdminDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
} 