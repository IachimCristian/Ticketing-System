using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TicketingSystem.Core.Services;

namespace TicketingSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly TicketPurchaseFacade _ticketPurchaseFacade;

        public TicketsController(TicketPurchaseFacade ticketPurchaseFacade)
        {
            _ticketPurchaseFacade = ticketPurchaseFacade;
        }

        [HttpPost]
        public async Task<IActionResult> PurchaseTicket([FromBody] PurchaseTicketDto purchaseDto)
        {
            try
            {
                var ticket = await _ticketPurchaseFacade.PurchaseTicketAsync(
                    purchaseDto.EventId,
                    purchaseDto.CustomerId,
                    purchaseDto.PaymentMethod);

                return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticket);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetTicket(Guid id)
        {
            // This is a placeholder for a real implementation
            // You would need to add a method to get a ticket by ID to the facade or use a repository
            return Ok(new { Id = id, Message = "Ticket retrieval not yet implemented" });
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelTicket(Guid id, [FromBody] CancelTicketDto cancelDto)
        {
            try
            {
                var result = await _ticketPurchaseFacade.CancelTicketAsync(id, cancelDto.CustomerId);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
    }

    public class PurchaseTicketDto
    {
        public Guid EventId { get; set; }
        public Guid CustomerId { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class CancelTicketDto
    {
        public Guid CustomerId { get; set; }
    }
} 