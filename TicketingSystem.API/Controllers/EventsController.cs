using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Services;

namespace TicketingSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly EventService _eventService;
        private readonly EventBuilder _eventBuilder;

        public EventsController(EventService eventService)
        {
            _eventService = eventService;
            _eventBuilder = new EventBuilder();
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _eventService.GetUpcomingEventsAsync();
            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(Guid id)
        {
            var @event = await _eventService.GetEventByIdAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            return Ok(@event);
        }

        [HttpGet("organizer/{organizerId}")]
        public async Task<IActionResult> GetEventsByOrganizer(Guid organizerId)
        {
            var events = await _eventService.GetEventsByOrganizerAsync(organizerId);
            return Ok(events);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchEvents([FromQuery] string searchTerm)
        {
            var events = await _eventService.SearchEventsAsync(searchTerm);
            return Ok(events);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] EventDto eventDto)
        {
            var @event = _eventBuilder
                .WithTitle(eventDto.Title)
                .WithDescription(eventDto.Description)
                .WithDates(eventDto.StartDate, eventDto.EndDate)
                .AtLocation(eventDto.Location)
                .WithCapacity(eventDto.TotalTickets)
                .WithTicketPrice(eventDto.TicketPrice)
                .WithImage(eventDto.ImageUrl)
                .ByOrganizer(eventDto.OrganizerId)
                .IsActive(true)
                .Build();

            var createdEvent = await _eventService.CreateEventAsync(@event);
            return CreatedAtAction(nameof(GetEvent), new { id = createdEvent.Id }, createdEvent);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] EventDto eventDto)
        {
            var @event = await _eventService.GetEventByIdAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            @event.Title = eventDto.Title;
            @event.Description = eventDto.Description;
            @event.StartDate = eventDto.StartDate;
            @event.EndDate = eventDto.EndDate;
            @event.Location = eventDto.Location;
            @event.TotalTickets = eventDto.TotalTickets;
            @event.AvailableTickets = eventDto.AvailableTickets;
            @event.TicketPrice = eventDto.TicketPrice;

            await _eventService.UpdateEventAsync(@event);
            return Ok(@event);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelEvent(Guid id)
        {
            var result = await _eventService.CancelEventAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }

    public class EventDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }
        public int TotalTickets { get; set; }
        public int AvailableTickets { get; set; }
        public decimal TicketPrice { get; set; }
        public string ImageUrl { get; set; }
        public Guid OrganizerId { get; set; }
    }
} 