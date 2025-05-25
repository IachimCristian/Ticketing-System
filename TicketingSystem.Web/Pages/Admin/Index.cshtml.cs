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
    [IgnoreAntiforgeryToken]
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
                    UserType = "Customer"
                });
            }

            foreach (var organizer in organizers)
            {
                Users.Add(new UserViewModel
                {
                    Id = organizer.Id,
                    Username = organizer.Username,
                    Email = organizer.Email,
                    UserType = "Organizer"
                });
            }

            foreach (var admin in admins)
            {
                Users.Add(new UserViewModel
                {
                    Id = admin.Id,
                    Username = admin.Username,
                    Email = admin.Email,
                    UserType = "Admin"
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

            foreach (var user in Users)
            {
                // Sărim peste utilizatorul logat
                if (user.Username == currentUsername)
                    continue;

                string originalType = null;
                string password = "default";

                var existingCustomer = await _customerRepository.GetByIdAsync(user.Id);
                var existingOrganizer = await _organizerRepository.GetByIdAsync(user.Id);
                var existingAdmin = await _adminRepository.GetByIdAsync(user.Id);

                if (existingCustomer != null)
                {
                    originalType = "Customer";
                    password = existingCustomer.Password;
                }
                else if (existingOrganizer != null)
                {
                    originalType = "Organizer";
                    password = existingOrganizer.Password;
                }
                else if (existingAdmin != null)
                {
                    originalType = "Admin";
                    password = existingAdmin.Password;
                }

                if (user.UserType == originalType)
                {
                    if (existingCustomer != null)
                    {
                        existingCustomer.Email = user.Email;
                        await _customerRepository.UpdateAsync(existingCustomer);
                    }
                    else if (existingOrganizer != null)
                    {
                        existingOrganizer.Email = user.Email;
                        await _organizerRepository.UpdateAsync(existingOrganizer);
                    }
                    else if (existingAdmin != null)
                    {
                        existingAdmin.Email = user.Email;
                        await _adminRepository.UpdateAsync(existingAdmin);
                    }
                }
                else
                {
                    if (existingCustomer != null)
                        await _customerRepository.DeleteAsync(existingCustomer);
                    if (existingOrganizer != null)
                        await _organizerRepository.DeleteAsync(existingOrganizer);
                    if (existingAdmin != null)
                        await _adminRepository.DeleteAsync(existingAdmin);

                    if (user.UserType == "Customer")
                    {
                        var newCustomer = new TicketingSystem.Core.Entities.Customer
                        {
                            Id = user.Id,
                            Username = user.Username,
                            Email = user.Email,
                            Password = password,
                            Phone = existingOrganizer?.ContactPhone ?? "0000000000",
                            Address = existingOrganizer?.Description ?? "No address"
                        };
                    }
                    else if (user.UserType == "Organizer")
                    {
                        await _organizerRepository.AddAsync(new OrganizerEntity
                        {
                            Id = user.Id,
                            Username = user.Username,
                            Email = user.Email,
                            Password = password,
                            OrganizationName = "Converted Org",
                            ContactPhone = existingCustomer?.Phone ?? "0000000000",
                            Description = existingCustomer?.Address ?? "Converted from customer"
                        });
                    }
                    else if (user.UserType == "Admin")
                    {
                        await _adminRepository.AddAsync(new Administrator
                        {
                            Id = user.Id,
                            Username = user.Username,
                            Email = user.Email,
                            Password = password,
                            Role = "Standard",
                            IsActive = true
                        });
                    }
                }
            }

            await _customerRepository.SaveChangesAsync();
            await _organizerRepository.SaveChangesAsync();
            await _adminRepository.SaveChangesAsync();

            return RedirectToPage("Index", new { section = "users" });
        }

        public async Task<IActionResult> OnPostRemoveAsync(Guid id, string userType)
        {
            if (userType == "Customer")
            {
                var customer = await _customerRepository.GetByIdAsync(id);
                if (customer != null)
                {
                    await _customerRepository.DeleteAsync(customer);
                    await _customerRepository.SaveChangesAsync();
                }
            }
            else if (userType == "Organizer")
            {
                var organizer = await _organizerRepository.GetByIdAsync(id);
                if (organizer != null)
                {
                    await _organizerRepository.DeleteAsync(organizer);
                    await _organizerRepository.SaveChangesAsync();
                }
            }
            else if (userType == "Admin")
            {
                var admin = await _adminRepository.GetByIdAsync(id);
                if (admin != null)
                {
                    await _adminRepository.DeleteAsync(admin);
                    await _adminRepository.SaveChangesAsync();
                }
            }

            return new JsonResult(new { success = true });
        }

        public class UserViewModel
        {
            public Guid Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string UserType { get; set; }
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