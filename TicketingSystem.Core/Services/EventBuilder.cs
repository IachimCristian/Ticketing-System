using System;
using TicketingSystem.Core.Entities;

namespace TicketingSystem.Core.Services
{
    public class EventBuilder
    {
        private readonly Event _event;
        
        public EventBuilder()
        {
            _event = new Event();
        }
        
        public EventBuilder WithTitle(string title)
        {
            _event.Title = title;
            return this;
        }
        
        public EventBuilder WithDescription(string description)
        {
            _event.Description = description;
            return this;
        }
        
        public EventBuilder WithDates(DateTime startDate, DateTime endDate)
        {
            _event.StartDate = startDate;
            _event.EndDate = endDate;
            return this;
        }
        
        public EventBuilder AtLocation(string location)
        {
            _event.Location = location;
            return this;
        }
        
        public EventBuilder WithCapacity(int capacity)
        {
            _event.Capacity = capacity;
            return this;
        }
        
        public EventBuilder WithTicketPrice(decimal price)
        {
            _event.TicketPrice = price;
            return this;
        }
        
        public EventBuilder WithImage(string imageUrl)
        {
            // ImageUrl property has been removed from Event entity
            // This method is kept for backward compatibility but does nothing
            return this;
        }
        
        public EventBuilder ByOrganizer(Guid organizerId)
        {
            _event.OrganizerId = organizerId;
            return this;
        }
        
        public EventBuilder IsActive(bool isActive)
        {
            _event.IsActive = isActive;
            return this;
        }
        
        public Event Build()
        {
            return _event;
        }
    }
} 