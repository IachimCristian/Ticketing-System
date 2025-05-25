using System;
using System.Collections.Generic;

namespace TicketingSystem.Core.Entities
{
    public class SeatMap
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public virtual Event Event { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public string SeatLayout { get; set; } // JSON representation of seat availability
        
        // Store information about different seat sections (e.g., A, B, C)
        public virtual ICollection<SeatSection> Sections { get; set; }
        
        public SeatMap()
        {
            Id = Guid.NewGuid();
            Sections = new List<SeatSection>();
        }
        
        public string GetSeatLabel(int row, int column)
        {
            // Returns a human-readable seat label (e.g., "A12")
            string rowLabel = row.ToString();
            string columnLabel = ((char)('A' + column)).ToString();
            return $"{columnLabel}{rowLabel}";
        }
    }
    
    public class SeatSection
    {
        public Guid Id { get; set; }
        public string Name { get; set; } // e.g., "A", "B", "Premium"
        public decimal PriceMultiplier { get; set; } // Price adjustment for this section
        public string Color { get; set; } // For UI display
        public int StartRow { get; set; }
        public int EndRow { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
        public Guid SeatMapId { get; set; } // Foreign key to SeatMap
        public virtual SeatMap SeatMap { get; set; } // Navigation property
        
        public SeatSection()
        {
            Id = Guid.NewGuid();
        }
    }
} 