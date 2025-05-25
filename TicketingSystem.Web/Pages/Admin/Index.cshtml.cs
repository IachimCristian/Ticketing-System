using Microsoft.AspNetCore.Mvc;
using OrganizerEntity = TicketingSystem.Core.Entities.Organizer;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using TicketingSystem.Core.Entities;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Web.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly IRepository<TicketingSystem.Core.Entities.Customer> _customerRepository;
        private readonly IRepository<TicketingSystem.Core.Entities.Organizer> _organizerRepository;
        private readonly IRepository<Administrator> _adminRepository;
        private readonly IRepository<Event> _eventRepository;
        private readonly IRepository<Payment> _paymentRepository;
        private readonly IRepository<Ticket> _ticketRepository;

        public int UserCount { get; set; }
        public int EventCount { get; set; }
        public int TransactionCount { get; set; }
        public string CurrentAdminUsername { get; set; }

        [BindProperty]
        public List<UserViewModel> Users { get; set; }

        public List<EventViewModel> Events { get; set; }
        public List<TransactionViewModel> Transactions { get; set; }

        public IndexModel(
            IRepository<TicketingSystem.Core.Entities.Customer> customerRepository,
            IRepository<TicketingSystem.Core.Entities.Organizer> organizerRepository,
            IRepository<Administrator> adminRepository,
            IRepository<Event> eventRepository,
            IRepository<Payment> paymentRepository,
            IRepository<Ticket> ticketRepository)
        {
            _customerRepository = customerRepository;
            _organizerRepository = organizerRepository;
            _adminRepository = adminRepository;
            _eventRepository = eventRepository;
            _paymentRepository = paymentRepository;
            _ticketRepository = ticketRepository;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Admin")
            {
                return RedirectToPage("/Index");
            }

            CurrentAdminUsername = HttpContext.Session.GetString("Username");

            var customers = await _customerRepository.GetAllAsync();
            var organizers = await _organizerRepository.GetAllAsync();
            var admins = await _adminRepository.GetAllAsync();

            UserCount = customers.Count() + organizers.Count() + admins.Count();

            var events = await _eventRepository.GetAllAsync();
            EventCount = events.Count();

            var transactions = await _paymentRepository.GetAllAsync();
            TransactionCount = transactions.Count();

            Users = new List<UserViewModel>();

            foreach (var customer in customers)
            {
                Users.Add(new UserViewModel
                {
                    Id = customer.Id,
                    Username = customer.Username,
                    Email = customer.Email,
                    UserType = "Customer",
                    OriginalUserType = "Customer"
                });
            }

            foreach (var organizer in organizers)
            {
                Users.Add(new UserViewModel
                {
                    Id = organizer.Id,
                    Username = organizer.Username,
                    Email = organizer.Email,
                    UserType = "Organizer",
                    OriginalUserType = "Organizer"
                });
            }

            foreach (var admin in admins)
            {
                Users.Add(new UserViewModel
                {
                    Id = admin.Id,
                    Username = admin.Username,
                    Email = admin.Email,
                    UserType = "Admin",
                    OriginalUserType = "Admin"
                });
            }

            Events = events.Select(e => new EventViewModel
            {
                Title = e.Title,
                Date = e.StartDate,
                Location = e.Location
            }).ToList();

            Transactions = new List<TransactionViewModel>();

            foreach (var payment in transactions)
            {
                var tickets = await _ticketRepository.GetAllAsync();
                var paymentTickets = tickets.Where(t => t.PaymentId == payment.Id).ToList();

                var eventTitle = "Unknown";
                if (paymentTickets.Any() && paymentTickets.First().EventId != Guid.Empty)
                {
                    var eventItem = await _eventRepository.GetByIdAsync(paymentTickets.First().EventId);
                    if (eventItem != null)
                    {
                        eventTitle = eventItem.Title;
                    }
                }

                Transactions.Add(new TransactionViewModel
                {
                    Id = payment.Id,
                    Amount = payment.Amount,
                    Date = payment.TransactionDate,
                    Username = payment.Customer?.Username ?? "Unknown",
                    EventTitle = eventTitle
                });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var currentUsername = HttpContext.Session.GetString("Username");

            // Debug: Log what we received
            Console.WriteLine($"Received {Users?.Count ?? 0} users in the form");
            if (Users != null)
            {
                foreach (var user in Users)
                {
                    Console.WriteLine($"User: {user.Username}, Original: {user.OriginalUserType}, New: {user.UserType}, Email: {user.Email}");
                }
            }

            foreach (var user in Users)
            {
                // Skip the current admin - they cannot modify themselves
                if (user.Username == currentUsername)
                    continue;

                try
                {
                    // Handle role changes
                    if (user.UserType != user.OriginalUserType)
                    {
                        // Log the role change attempt
                        Console.WriteLine($"Attempting to change user {user.Username} from {user.OriginalUserType} to {user.UserType}");
                        await ChangeUserRole(user);
                        Console.WriteLine($"Successfully changed user {user.Username} to {user.UserType}");
                    }
                    else
                    {
                        // Just update email if no role change
                        await UpdateUserEmail(user);
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue with other users
                    Console.WriteLine($"Error updating user {user.Username}: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    ModelState.AddModelError("", $"Error updating user {user.Username}: {ex.Message}");
                }
            }

            // If there are errors, stay on the same page to show them
            if (!ModelState.IsValid)
            {
                // Reload the data
                await OnGetAsync();
                ViewData["Section"] = "users";
                return Page();
            }

            return RedirectToPage("Index", new { section = "users" });
        }

        private async Task HandleOrganizerEvents(Guid organizerId)
        {
            var organizerEvents = await _eventRepository.GetAllAsync();
            var userEvents = organizerEvents.Where(e => e.OrganizerId == organizerId).ToList();
            
            if (!userEvents.Any())
                return;

            // Option 1: Create or find a "System" organizer to transfer events to
            var systemOrganizer = await GetOrCreateSystemOrganizer();
            
            if (systemOrganizer == null)
            {
                // Option 2: If we can't create a system organizer, we need to delete the events
                // But first check if any events have tickets sold
                var allTickets = await _ticketRepository.GetAllAsync();
                var eventsWithTickets = userEvents.Where(e => allTickets.Any(t => t.EventId == e.Id)).ToList();
                
                if (eventsWithTickets.Any())
                {
                    throw new Exception($"Cannot change role: User has {eventsWithTickets.Count} events with sold tickets. Please handle these events first.");
                }
                
                // Delete events that have no tickets
                foreach (var evt in userEvents)
                {
                    await _eventRepository.DeleteAsync(evt);
                }
                await _eventRepository.SaveChangesAsync();
                Console.WriteLine($"Deleted {userEvents.Count} events (no tickets sold)");
                return;
            }

            // Transfer all events to the system organizer
            foreach (var evt in userEvents)
            {
                evt.OrganizerId = systemOrganizer.Id;
                await _eventRepository.UpdateAsync(evt);
            }
            
            await _eventRepository.SaveChangesAsync();
            Console.WriteLine($"Transferred {userEvents.Count} events to system organizer");
        }

        private async Task<OrganizerEntity> GetOrCreateSystemOrganizer()
        {
            try
            {
                // Look for existing system organizer
                var organizers = await _organizerRepository.GetAllAsync();
                var systemOrganizer = organizers.FirstOrDefault(o => o.Username == "system" || o.OrganizationName == "System");
                
                if (systemOrganizer != null)
                    return systemOrganizer;

                // Create a system organizer
                var newSystemOrganizer = new OrganizerEntity
                {
                    Id = Guid.NewGuid(),
                    Username = "system",
                    Email = "system@ticketingsystem.com",
                    Password = "SystemPassword123!",
                    OrganizationName = "System",
                    ContactPhone = "0000000000",
                    Description = "System organizer for transferred events",
                    CreatedAt = DateTime.UtcNow
                };

                await _organizerRepository.AddAsync(newSystemOrganizer);
                await _organizerRepository.SaveChangesAsync();
                
                Console.WriteLine("Created system organizer for event transfers");
                return newSystemOrganizer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create system organizer: {ex.Message}");
                return null;
            }
        }

        private async Task HandleCustomerTickets(Guid customerId)
        {
            var customerTickets = await _ticketRepository.GetAllAsync();
            var userTickets = customerTickets.Where(t => t.CustomerId == customerId).ToList();
            
            var customerPayments = await _paymentRepository.GetAllAsync();
            var userPayments = customerPayments.Where(p => p.CustomerId == customerId).ToList();
            
            if (!userTickets.Any() && !userPayments.Any())
                return;

            // Create or find a system customer to transfer tickets and payments to
            var systemCustomer = await GetOrCreateSystemCustomer();
            
            if (systemCustomer == null)
            {
                // If we can't create a system customer, prevent the role change
                throw new Exception($"Cannot change role: Customer has {userTickets.Count} tickets and {userPayments.Count} payments, but system customer could not be created.");
            }

            // Transfer all tickets to the system customer
            foreach (var ticket in userTickets)
            {
                ticket.CustomerId = systemCustomer.Id;
                await _ticketRepository.UpdateAsync(ticket);
            }
            
            if (userTickets.Any())
            {
                await _ticketRepository.SaveChangesAsync();
                Console.WriteLine($"Transferred {userTickets.Count} tickets to system customer");
            }

            // Transfer all payments to the system customer
            foreach (var payment in userPayments)
            {
                payment.CustomerId = systemCustomer.Id;
                await _paymentRepository.UpdateAsync(payment);
            }
            
            if (userPayments.Any())
            {
                await _paymentRepository.SaveChangesAsync();
                Console.WriteLine($"Transferred {userPayments.Count} payments to system customer");
            }
        }

        private async Task<TicketingSystem.Core.Entities.Customer> GetOrCreateSystemCustomer()
        {
            try
            {
                // Look for existing system customer
                var customers = await _customerRepository.GetAllAsync();
                var systemCustomer = customers.FirstOrDefault(c => c.Username == "system-customer" || c.Email == "system-customer@ticketingsystem.com");
                
                if (systemCustomer != null)
                    return systemCustomer;

                // Create a system customer
                var newSystemCustomer = new TicketingSystem.Core.Entities.Customer
                {
                    Id = Guid.NewGuid(),
                    Username = "system-customer",
                    Email = "system-customer@ticketingsystem.com",
                    Password = "SystemPassword123!",
                    Phone = "0000000000",
                    Address = "System address for transferred tickets",
                    CreatedAt = DateTime.UtcNow
                };

                await _customerRepository.AddAsync(newSystemCustomer);
                await _customerRepository.SaveChangesAsync();
                
                Console.WriteLine("Created system customer for ticket transfers");
                return newSystemCustomer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create system customer: {ex.Message}");
                return null;
            }
        }

        private async Task ChangeUserRole(UserViewModel user)
        {
            // Get the original user data
            var originalCustomer = await _customerRepository.GetByIdAsync(user.Id);
            var originalOrganizer = await _organizerRepository.GetByIdAsync(user.Id);
            var originalAdmin = await _adminRepository.GetByIdAsync(user.Id);

            string password = "defaultPassword123"; // Default password for new users
            string phone = "0000000000";
            string address = "No address provided";
            string organizationName = "Default Organization";
            string description = "Converted user";

            // Extract existing data based on original user type
            if (originalCustomer != null)
            {
                password = originalCustomer.Password;
                phone = originalCustomer.Phone;
                address = originalCustomer.Address;
            }
            else if (originalOrganizer != null)
            {
                password = originalOrganizer.Password;
                phone = originalOrganizer.ContactPhone ?? "0000000000";
                organizationName = originalOrganizer.OrganizationName ?? "Default Organization";
                description = originalOrganizer.Description ?? "Converted from organizer";
                // For organizer to customer conversion, use organizer data as address
                address = $"{organizationName} - {description}";
            }
            else if (originalAdmin != null)
            {
                password = originalAdmin.Password;
                // For admin conversions, use default values
            }

            // Handle all related data (tickets, payments, events) before deleting user
            await HandleAllUserRelatedData(user);

            // Delete the original user first
            try
            {
                if (originalCustomer != null)
                {
                    await _customerRepository.DeleteAsync(originalCustomer);
                    await _customerRepository.SaveChangesAsync();
                }
                if (originalOrganizer != null)
                {
                    await _organizerRepository.DeleteAsync(originalOrganizer);
                    await _organizerRepository.SaveChangesAsync();
                }
                if (originalAdmin != null)
                {
                    await _adminRepository.DeleteAsync(originalAdmin);
                    await _adminRepository.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete original user: {ex.Message}");
            }

            // Create the new user with the new role
            try
            {
                switch (user.UserType)
                {
                    case "Customer":
                        var newCustomer = new TicketingSystem.Core.Entities.Customer
                        {
                            Id = user.Id,
                            Username = user.Username,
                            Email = user.Email,
                            Password = password,
                            Phone = phone,
                            Address = address,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _customerRepository.AddAsync(newCustomer);
                        await _customerRepository.SaveChangesAsync();
                        break;

                    case "Organizer":
                        var newOrganizer = new OrganizerEntity
                        {
                            Id = user.Id,
                            Username = user.Username,
                            Email = user.Email,
                            Password = password,
                            OrganizationName = organizationName,
                            ContactPhone = phone,
                            Description = description,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _organizerRepository.AddAsync(newOrganizer);
                        await _organizerRepository.SaveChangesAsync();
                        break;

                    case "Admin":
                        var newAdmin = new Administrator
                        {
                            Id = user.Id,
                            Username = user.Username,
                            Email = user.Email,
                            Password = password,
                            Role = "Standard",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _adminRepository.AddAsync(newAdmin);
                        await _adminRepository.SaveChangesAsync();
                        break;

                    default:
                        throw new Exception($"Invalid user type: {user.UserType}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create new user with role {user.UserType}: {ex.Message}");
            }
        }

        private async Task UpdateUserEmail(UserViewModel user)
        {
            switch (user.UserType)
            {
                case "Customer":
                    var customer = await _customerRepository.GetByIdAsync(user.Id);
                    if (customer != null)
                    {
                        customer.Email = user.Email;
                        await _customerRepository.UpdateAsync(customer);
                        await _customerRepository.SaveChangesAsync();
                    }
                    break;

                case "Organizer":
                    var organizer = await _organizerRepository.GetByIdAsync(user.Id);
                    if (organizer != null)
                    {
                        organizer.Email = user.Email;
                        await _organizerRepository.UpdateAsync(organizer);
                        await _organizerRepository.SaveChangesAsync();
                    }
                    break;

                case "Admin":
                    var admin = await _adminRepository.GetByIdAsync(user.Id);
                    if (admin != null)
                    {
                        admin.Email = user.Email;
                        await _adminRepository.UpdateAsync(admin);
                        await _adminRepository.SaveChangesAsync();
                    }
                    break;
            }
        }

        public async Task<IActionResult> OnPostRemoveAsync(Guid id, string userType)
        {
            var currentUsername = HttpContext.Session.GetString("Username");
            
            // Get user details to check if it's a system account
            var userToDelete = await GetUserById(id, userType);
            
            // Prevent current admin from deleting themselves
            if (userToDelete?.Username == currentUsername)
            {
                return new JsonResult(new { success = false, message = "You cannot delete yourself!" });
            }

            // Prevent deletion of system accounts
            if (userToDelete?.Username == "system" || userToDelete?.Username == "system-customer")
            {
                return new JsonResult(new { success = false, message = "System accounts cannot be deleted as they are required for system functionality." });
            }

            try
            {
                // Handle all related data before deletion
                var userViewModel = new UserViewModel 
                { 
                    Id = id, 
                    OriginalUserType = userType 
                };
                
                await HandleAllUserRelatedData(userViewModel);

                // Now safely delete the user
                switch (userType)
                {
                    case "Customer":
                        var customer = await _customerRepository.GetByIdAsync(id);
                        if (customer != null)
                        {
                            await _customerRepository.DeleteAsync(customer);
                            await _customerRepository.SaveChangesAsync();
                        }
                        break;

                    case "Organizer":
                        var organizer = await _organizerRepository.GetByIdAsync(id);
                        if (organizer != null)
                        {
                            await _organizerRepository.DeleteAsync(organizer);
                            await _organizerRepository.SaveChangesAsync();
                        }
                        break;

                    case "Admin":
                        var admin = await _adminRepository.GetByIdAsync(id);
                        if (admin != null)
                        {
                            await _adminRepository.DeleteAsync(admin);
                            await _adminRepository.SaveChangesAsync();
                        }
                        break;
                }

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        private async Task<dynamic> GetUserById(Guid id, string userType)
        {
            switch (userType)
            {
                case "Customer":
                    return await _customerRepository.GetByIdAsync(id);
                case "Organizer":
                    return await _organizerRepository.GetByIdAsync(id);
                case "Admin":
                    return await _adminRepository.GetByIdAsync(id);
                default:
                    return null;
            }
        }

        private async Task HandleAllUserRelatedData(UserViewModel user)
        {
            // Handle based on original user type
            switch (user.OriginalUserType)
            {
                case "Customer":
                    await HandleCustomerTickets(user.Id);
                    break;
                    
                case "Organizer":
                    await HandleOrganizerEvents(user.Id);
                    // Organizers might also have payments if they were customers before
                    await HandleAnyUserPayments(user.Id);
                    break;
                    
                case "Admin":
                    // Admins might have payments if they were customers before
                    await HandleAnyUserPayments(user.Id);
                    break;
            }
        }

        private async Task HandleAnyUserPayments(Guid userId)
        {
            // Check if this user has any payments (regardless of current type)
            var allPayments = await _paymentRepository.GetAllAsync();
            var userPayments = allPayments.Where(p => p.CustomerId == userId).ToList();
            
            if (!userPayments.Any())
                return;

            // Create or find a system customer to transfer payments to
            var systemCustomer = await GetOrCreateSystemCustomer();
            
            if (systemCustomer == null)
            {
                throw new Exception($"Cannot change role: User has {userPayments.Count} payments, but system customer could not be created.");
            }

            // Transfer all payments to the system customer
            foreach (var payment in userPayments)
            {
                payment.CustomerId = systemCustomer.Id;
                await _paymentRepository.UpdateAsync(payment);
            }
            
            await _paymentRepository.SaveChangesAsync();
            Console.WriteLine($"Transferred {userPayments.Count} payments to system customer");
        }

        public class UserViewModel
        {
            public Guid Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string UserType { get; set; }
            public string OriginalUserType { get; set; }
        }

        public class EventViewModel
        {
            public string Title { get; set; }
            public DateTime Date { get; set; }
            public string Location { get; set; }
        }

        public class TransactionViewModel
        {
            public Guid Id { get; set; }
            public string Username { get; set; }
            public string EventTitle { get; set; }
            public decimal Amount { get; set; }
            public DateTime Date { get; set; }
        }
    }
}