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
        
        public bool IsSeatAvailable(int row, int column)
        {
            // In a real implementation, this would check the SeatLayout
            // to determine if a seat is available
            
            // Create a pattern of unavailable seats similar to the image
            // Diagonal pattern of unavailable seats
            if ((row + column) % 3 == 0)
                return false;
                
            // First two rows have more unavailable seats
            if (row <= 2 && column % 2 == 1)
                return false;
                
            // Some specific seats unavailable
            if (row == 3 && (column == 2 || column == 4))
                return false;
                
            // All other seats are available
            return true;
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