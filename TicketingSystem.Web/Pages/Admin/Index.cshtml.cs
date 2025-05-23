using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TicketingSystem.Core.Interfaces;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Web.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<TicketingSystem.Core.Entities.Organizer> _organizerRepository;
        private readonly IRepository<Administrator> _adminRepository;
        private readonly IRepository<Event> _eventRepository;
        private readonly IRepository<Payment> _paymentRepository;
        private readonly IRepository<Ticket> _ticketRepository;

        public int UserCount { get; set; }
        public int EventCount { get; set; }
        public int TransactionCount { get; set; }

        public List<UserViewModel> Users { get; set; }
        public List<EventViewModel> Events { get; set; }
        public List<TransactionViewModel> Transactions { get; set; }

        public IndexModel(
            IRepository<Customer> customerRepository,
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
            // Check if user is admin
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Admin")
            {
                return RedirectToPage("/Index");
            }

            // Get counts
            var customers = await _customerRepository.GetAllAsync();
            var organizers = await _organizerRepository.GetAllAsync();
            var admins = await _adminRepository.GetAllAsync();
            
            UserCount = customers.Count() + organizers.Count() + admins.Count();
            
            var events = await _eventRepository.GetAllAsync();
            EventCount = events.Count();
            
            var transactions = await _paymentRepository.GetAllAsync();
            TransactionCount = transactions.Count();

            // Populate user list
            Users = new List<UserViewModel>();
            
            foreach (var customer in customers)
            {
                Users.Add(new UserViewModel { 
                    Username = customer.Username,
                    Email = customer.Email,
                    UserType = "Customer"
                });
            }
            
            foreach (var organizer in organizers)
            {
                Users.Add(new UserViewModel { 
                    Username = organizer.Username,
                    Email = organizer.Email,
                    UserType = "Organizer"
                });
            }
            
            foreach (var admin in admins)
            {
                Users.Add(new UserViewModel { 
                    Username = admin.Username,
                    Email = admin.Email,
                    UserType = "Admin"
                });
            }

            // Populate events
            Events = events.Select(e => new EventViewModel
            {
                Title = e.Title,
                Date = e.StartDate,
                Location = e.Location
            }).ToList();

            // Populate transactions
            Transactions = new List<TransactionViewModel>();
            
            foreach (var payment in transactions)
            {
                // Get the first ticket associated with this payment (if any)
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
                    Date = payment.PaymentDate,
                    Username = payment.Customer?.Username ?? "Unknown",
                    EventTitle = eventTitle
                });
            }

            return Page();
        }

        public class UserViewModel
        {
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