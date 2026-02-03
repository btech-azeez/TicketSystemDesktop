using Microsoft.AspNetCore.Mvc;
using TicketSystemAPI.Models;
using TicketSystemAPI.Services;

namespace TicketSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly DatabaseService _dbService;

        public TicketsController(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<Ticket>>> CreateTicket([FromBody] CreateTicketRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Subject))
                {
                    return BadRequest(new ApiResponse<Ticket>
                    {
                        Success = false,
                        Message = "Subject is required"
                    });
                }

                if (string.IsNullOrEmpty(request.Description))
                {
                    return BadRequest(new ApiResponse<Ticket>
                    {
                        Success = false,
                        Message = "Description is required"
                    });
                }

                var ticket = await _dbService.CreateTicket(request);

                return Ok(new ApiResponse<Ticket>
                {
                    Success = true,
                    Message = "Ticket created successfully",
                    Data = ticket
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<Ticket>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<ApiResponse<List<Ticket>>>> GetUserTickets(int userId)
        {
            try
            {
                var tickets = await _dbService.GetTicketsByUser(userId);
                return Ok(new ApiResponse<List<Ticket>>
                {
                    Success = true,
                    Message = "Tickets retrieved successfully",
                    Data = tickets
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<Ticket>>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }

        [HttpGet("all")]
        public async Task<ActionResult<ApiResponse<List<Ticket>>>> GetAllTickets()
        {
            try
            {
                var tickets = await _dbService.GetAllTickets();
                return Ok(new ApiResponse<List<Ticket>>
                {
                    Success = true,
                    Message = "All tickets retrieved successfully",
                    Data = tickets
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<Ticket>>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }

        [HttpGet("{ticketId}")]
        public async Task<ActionResult<ApiResponse<TicketDetailsResponse>>> GetTicketDetails(int ticketId)
        {
            try
            {
                var ticket = await _dbService.GetTicketById(ticketId);
                
                if (ticket == null)
                {
                    return NotFound(new ApiResponse<TicketDetailsResponse>
                    {
                        Success = false,
                        Message = "Ticket not found"
                    });
                }

                var history = await _dbService.GetTicketHistory(ticketId);
                var comments = await _dbService.GetTicketComments(ticketId);

                var response = new TicketDetailsResponse
                {
                    Ticket = ticket,
                    StatusHistory = history,
                    Comments = comments
                };

                return Ok(new ApiResponse<TicketDetailsResponse>
                {
                    Success = true,
                    Message = "Ticket details retrieved successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<TicketDetailsResponse>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }

        [HttpPut]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateTicket([FromBody] UpdateTicketRequest request)
        {
            try
            {
                var result = await _dbService.UpdateTicket(request);

                if (result)
                {
                    return Ok(new ApiResponse<bool>
                    {
                        Success = true,
                        Message = "Ticket updated successfully",
                        Data = true
                    });
                }

                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Failed to update ticket. Ticket may be closed or not found."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }

        [HttpPost("comment")]
        public async Task<ActionResult<ApiResponse<bool>>> AddComment([FromBody] AddCommentRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.CommentText))
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Comment text is required"
                    });
                }

                var result = await _dbService.AddComment(request, null);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Comment added successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }
    }
}
